using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace UnifyVersions
{
    public class UnifyVersionsWorker
    {
        public static bool UnifyVersion(string rootDirectory, bool checkOnly)
        {
            string[] files = Directory.GetFiles(rootDirectory, "*.csproj", SearchOption.AllDirectories);

            //
            // Collects package info.
            //

            List<Package> packages = new List<Package>();

            foreach (string file in files)
            {
                var document = XDocument.Parse(File.ReadAllText(file));

                var packageReferenceList = new List<XElement>();
                packageReferenceList.AddRange(document.Root.Elements("ItemGroup").SelectMany(p => p.Elements("PackageReference")));
                packageReferenceList.AddRange(document.Root.Elements("ItemGroup").SelectMany(p => p.Elements("DotNetCliToolReference")));

                foreach (var packageReference in packageReferenceList)
                {
                    string include = packageReference.Attribute("Include")?.Value ?? packageReference.Attribute("include")?.Value;

                    string version = packageReference.Attribute("Version")?.Value ?? packageReference.Attribute("version")?.Value;

                    if (string.IsNullOrEmpty(include) || string.IsNullOrEmpty(version))
                    {
                        Console.WriteLine($"Invalid package reference: {packageReference.ToString()}");
                        if (checkOnly)
                        {
                            return false;
                        }
                        continue;
                    }

                    if (StringComparer.OrdinalIgnoreCase.Equals(version, GetReferencedPackageVersionProperty(include)))
                    {
                        continue;
                    }

                    if (checkOnly)
                    {
                        Console.Error.WriteLine($"Invalid version reference in file: {file}");
                        Console.Error.WriteLine(packageReference.ToString());
                    }
                    packages.Add(new Package() { Id = include, Version = version });
                }
            }

            if(packages.Count==0)
            {
                return true;
            }
            if(checkOnly)
            {
                return false;
            }

            var uniquePackages = packages.Distinct(PackageComparer.Default).ToList();
            uniquePackages.Sort(PackageComparer.Default);

            //
            // Rewrites project files with version properties.
            //

            foreach (string file in files)
            {
                var document = XDocument.Parse(File.ReadAllText(file));

                var packageReferenceList = new List<XElement>();
                packageReferenceList.AddRange(document.Root.Elements("ItemGroup").SelectMany(p => p.Elements("PackageReference")));
                packageReferenceList.AddRange(document.Root.Elements("ItemGroup").SelectMany(p => p.Elements("DotNetCliToolReference")));

                foreach (var packageReference in packageReferenceList)
                {
                    string include = packageReference.Attribute("Include")?.Value ?? packageReference.Attribute("include")?.Value;

                    var attribute = packageReference.Attribute("Version") ?? packageReference.Attribute("version");

                    if (string.IsNullOrEmpty(include) || attribute == null)
                    {
                        Console.WriteLine($"Invalid package reference: {packageReference.ToString()}");
                        continue;
                    }

                    attribute.Value = GetReferencedPackageVersionProperty(include);
                }

                document.Save(file);
            }

            Console.WriteLine("Copy the following to PackageVersions.props:");
            Console.WriteLine();

            var packageVersionProperties = uniquePackages.Select(p => $"<{GetPackageVersionProperty(p.Id)}>{p.Version}</{GetPackageVersionProperty(p.Id)}>").ToList();

            foreach (string packageVersionProperty in packageVersionProperties)
            {
                Console.WriteLine(packageVersionProperty);
            }

            Console.WriteLine("Completed.");

            return true;
        }

        private static string GetPackageVersionProperty(string packageId) => "PackageVersion_" + packageId.Replace(".", "_");

        private static string GetReferencedPackageVersionProperty(string packageId) => $"$({GetPackageVersionProperty(packageId)})";

        private class Package
        {
            public string Id { get; set; }

            public string Version { get; set; }
        }

        private class PackageComparer : IComparer<Package>, IEqualityComparer<Package>
        {
            public static readonly PackageComparer Default = new PackageComparer();

            public int Compare(Package x, Package y)
            {
                int result = 0;

                result = StringComparer.OrdinalIgnoreCase.Compare(x.Id, y.Id);
                if (result != 0)
                {
                    return result;
                }

                result = StringComparer.OrdinalIgnoreCase.Compare(x.Version, y.Version);
                if (result != 0)
                {
                    return result;
                }

                return result;
            }

            public bool Equals(Package x, Package y)
            {
                return Compare(x, y) == 0;
            }

            public int GetHashCode(Package obj)
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Id) ^
                       StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Version);
            }
        }
    }
}
