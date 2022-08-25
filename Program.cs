﻿using System.Text.Json;

namespace atlas
{
    class Program
    {
        public static Dictionary<string, string> ExtensionToMimeType = new();
        public static Dictionary<string, string> MimeTypeToExtension = new();

        static void Main()
        {
            LoadMimeMap();
            LoadConfig();
            Console.WriteLine("Atlas ready!");
            Server.Start();

            while(true)
                Thread.Sleep(int.MaxValue);
        }

        private static void LoadConfig()
        {
            if (File.Exists("config.json"))
                Server.Config = JsonSerializer.Deserialize<Configuration>(File.ReadAllText("config.json"));
            else if (File.Exists("/etc/atlas/config.json"))
                Server.Config = JsonSerializer.Deserialize<Configuration>(File.ReadAllText("/etc/atlas/config.json"));
            if (Server.Config == null)
            {
                Console.WriteLine("Failed to load configuration. Does config.json exist?");
                Console.WriteLine($"Looking @ '/etc/atlas/config.json'");
                Console.WriteLine($"Looking @ '{Environment.CurrentDirectory}/config.json'");
                Console.WriteLine($"");
                Console.WriteLine($"--- Creating Default Configuration ---");
                Console.WriteLine($"");
                Server.Config = new Configuration()
                {
                    Port = 1965,
                    Capsules = new()
                    {
                        [Environment.MachineName] = new Capsule()
                        {
                            FQDN = Environment.MachineName,
                            AbsoluteTlsCertPath = $"/srv/gemini/{Environment.MachineName}/{Environment.MachineName}.pfx",
                            AbsoluteRootPath = $"/srv/gemini/{Environment.MachineName}/",
                            MaxUploadSize = 1024*1024*4,
                            Index = "index.gmi",
                            Locations = new()
                            {
                                new Location()
                                {
                                    Index = "index.gmi",
                                    AbsoluteRootPath = $"/srv/gemini/{Environment.MachineName}/",
                                },
                                new Location()
                                {
                                    Index = "script.sh",
                                    CGI = true,
                                    AbsoluteRootPath = $"/srv/gemini/{Environment.MachineName}/cgi/",
                                    RequireClientCert = true
                                },
                                new Location()
                                {
                                    AbsoluteRootPath = $"/srv/gemini/{Environment.MachineName}/files/",
                                    DirectoryListing = true,
                                    AllowFileUploads = true,

                                    AllowedMimeTypes = new()
                                    {
                                        ["text/*"] = new MimeConfig
                                        {
                                            MaxSizeBytes = 1024 * 1024 * 1, 
                                        },
                                        ["image/*"] = new MimeConfig{},
                                        ["audio/mpeg"] = new MimeConfig{},
                                        ["audio/ogg"] = new MimeConfig{},
                                    }
                                },
                            }
                        }
                    }

                };
                var json = JsonSerializer.Serialize(Server.Config, new JsonSerializerOptions() 
                { 
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Always,
                    IncludeFields=true,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
                });

                Console.WriteLine(json);
                Environment.Exit(0);
            }
        }

        private static void LoadMimeMap()
        {
            var lines = File.ReadLines("mimetypes.tsv");
            foreach (var line in lines)
            {
                var parts = line.Split('\t');
                ExtensionToMimeType.Add(parts[0], parts[1]);
            }
        }
    }
}