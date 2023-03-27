using Amazon.EC2.Model;
using Amazon.EC2;
using RestSharp;

namespace AWSTasks
{
    public class Tests
    {
        private IAmazonEC2 _amazonEC2 = new AmazonEC2Client();
        string ec2InstanceId = "";
        string ec2InstanceId2 = "";
        string publicIP = "";
        string privateIP = "";
        string privateIP2 = "";

        [Test]
        public async Task Task3()
        {
            var instance = await DescribeInstance(ec2InstanceId);

            StringAssert.AreEqualIgnoringCase("t2.micro", instance.InstanceType);

            var keys = instance.Tags.Select(x => x.Key).ToList();
            CollectionAssert.Contains(keys, "Name");
            CollectionAssert.Contains(keys, "cloudx");

            var volumeId = instance.BlockDeviceMappings[0].Ebs.VolumeId;
            var volume = await DescribeVolumeAsync(volumeId);

            Assert.AreEqual(8, volume.Size);
            Assert.AreEqual("Linux/UNIX", instance.PlatformDetails);
            Assert.AreEqual(publicIP, instance.PublicIpAddress);
            Assert.AreEqual(privateIP, instance.PrivateIpAddress);

            var instance2 = await DescribeInstance(ec2InstanceId2);

            Assert.AreEqual("t2.micro", instance2.InstanceType.Value);

            var keys2 = instance2.Tags.Select(x => x.Key).ToList();
            Assert.Contains("Name", keys2);
            Assert.Contains("cloudx", keys2);

            var volumeId2 = instance2.BlockDeviceMappings[0].Ebs.VolumeId;
            var volume2 = await DescribeVolumeAsync(volumeId2);

            Assert.AreEqual(8, volume2.Size);
            Assert.AreEqual("Linux/UNIX", instance2.PlatformDetails);
            Assert.Null(instance2.PublicIpAddress);
            Assert.AreEqual(privateIP2, instance2.PrivateIpAddress);


            var client = new RestClient($"http://{publicIP}/");
            var request = new RestRequest();
            var response = await client.GetAsync(request);

            string content = "{\n  \"availability_zone\": \"eu-central-1a\",\n  \"private_ipv4\": \"" + privateIP + "\",\n  \"region\": \"eu-central-1\"\n}\n";
            Assert.AreEqual(content, response.Content);
        }

        private async Task<Instance> DescribeInstance(string instanceId)
        {
            var response = await _amazonEC2.DescribeInstancesAsync(
                new DescribeInstancesRequest { InstanceIds = new List<string> { instanceId } });
            return response.Reservations[0].Instances[0];
        }

        public async Task<Volume> DescribeVolumeAsync(string volumeId)
        {
            var request = new DescribeVolumesRequest(new List<string> { volumeId });
            var response = await _amazonEC2.DescribeVolumesAsync(request);
            return response.Volumes[0];
        }

        public async Task<SecurityGroup> DescribeSecurityGroupAsync(string groupId)
        {
            var request = new DescribeSecurityGroupsRequest();
            var groupIds = new List<string> { groupId };
            request.GroupIds = groupIds;
            var response = await _amazonEC2.DescribeSecurityGroupsAsync(request);
            return response.SecurityGroups[0];
        }

        public async Task<List<SecurityGroupRule>> DescribeSecurityGroupRulesAsync(string groupId)
        {
            var request = new DescribeSecurityGroupRulesRequest() { SecurityGroupRuleIds = new List<string>() { groupId } };

            var response = await _amazonEC2.DescribeSecurityGroupRulesAsync(request);
            return response.SecurityGroupRules;
        }
    }
}