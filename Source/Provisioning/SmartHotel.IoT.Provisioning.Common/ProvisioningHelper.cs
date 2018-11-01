﻿using System;
using System.Collections.Generic;
using System.IO;
using SmartHotel.IoT.Provisioning.Common.Models;
using SmartHotel.IoT.Provisioning.Common.Models.DigitalTwins;
using YamlDotNet.Serialization;

namespace SmartHotel.IoT.Provisioning.Common
{
    public static class ProvisioningHelper
    {
	    public static ProvisioningDescription LoadSmartHotelProvisioning(string provisioningFilepath)
	    {
		    var yamlDeserializer = new Deserializer();
		    string content = File.ReadAllText(provisioningFilepath);
		    var provisioningDescription = yamlDeserializer.Deserialize<ProvisioningDescription>( content );

            string directoryPath = Path.GetDirectoryName(provisioningFilepath);

            foreach (SpaceDescription rootDescription in provisioningDescription.spaces)
            {
                var referenceSpaces = FindAllReferenceSpaces(rootDescription);
                foreach (var referenceSpace in referenceSpaces)
                {
                    LoadReferenceProvisionings(referenceSpace, directoryPath);
                }
            }

            return provisioningDescription;
	    }

        private static IEnumerable<SpaceDescription> FindAllReferenceSpaces(SpaceDescription parentDescription)
        {
            List<SpaceDescription> referenceSpaces = new List<SpaceDescription>();

            if (parentDescription.spaceReferences != null)
            {
                referenceSpaces.Add(parentDescription);
            }
            else
            {
                foreach (SpaceDescription childDescription in parentDescription.spaces)
                {
                    referenceSpaces.AddRange(FindAllReferenceSpaces(childDescription));
                }
            }

            return referenceSpaces;
        }

        public static void LoadReferenceProvisionings(SpaceDescription spaceDescription, string directoryPath)
        {
            var referenceSpaces = new List<SpaceDescription>();

            foreach (SpaceReferenceDescription spaceReferenceDescription in spaceDescription.spaceReferences)
            {
                if (String.IsNullOrEmpty(spaceReferenceDescription.filename))
                {
                    throw new Exception($"SpaceReference filename expected.");
                }

                string filepath = Path.Combine(directoryPath, spaceReferenceDescription.filename);

                var referenceSpace = LoadReferenceProvisioning(filepath);

                referenceSpaces.Add(referenceSpace);
            }

            spaceDescription.spaces = referenceSpaces;
        }

        public static SpaceDescription LoadReferenceProvisioning(string referenceFilename)
        {
            var yamlDeserializer = new Deserializer();
            string content = File.ReadAllText(Path.Combine("Resources", referenceFilename));
            return yamlDeserializer.Deserialize<SpaceDescription>(content);
        }
    }
}
