using Grpc.Core;
using GrpcServer;
using GrpcServer.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;
using System.Runtime.InteropServices.JavaScript;
using Newtonsoft.Json.Linq;

namespace GrpcServer.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ApplicationDbContext _db;

        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger, ApplicationDbContext context)
        {
            _db = context;
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }


        public override Task<HealthDataReply> GetHealthData(HealthDataRequest request, ServerCallContext context)
        {
            // Use the SingleOrDefault() method instead of First() to avoid an exception
            // if no matching data is found
            var healthData = _db.HealthData.SingleOrDefault(a =>
                a.Medical == request.Medical &&
                a.ChildDx == request.Childdx &&
                a.Selfharm == request.Selfharm &&
                a.Sofas == request.Sofas &&
                a.ClinicalStage == request.Clinicalstage &&
                a.Circadian == request.Circadian &&
                a.Tripartite == request.Tripartite &&
                a.Psychosis == request.Psychosis &&
                a.NEET == request.Neet
            );

            // Check if healthData is null, and return an error message if it is
            if (healthData == null)
            {
                return Task.FromResult(new HealthDataReply
                {
                    Message = "Error, cannot find data entry using the given parameters"
                });
            }

            // Create an anonymous object with the data we want to return
            var res = new
            {
                healthData.Alert,
                healthData.ChildDx,
                healthData.Circadian,
                healthData.ClinicalStage,
                healthData.Constant,
                healthData.Down
            };

            // Use the JsonSerializer.Serialize() method to convert the anonymous object to a JSON string
            return Task.FromResult(new HealthDataReply
            {
                Message = JsonSerializer.Serialize(res)
            });
        }


        public override Task<HealthDataReply> GetHealthDataJsonStringInput(HealthDataRequestJsonString request, ServerCallContext context)
        {
            var jsonStrings = new List<string>();
            var jsonDa = JsonSerializer.Deserialize<JsonElement>(request.Message);
            //error here trying to access the object like an array
            foreach (var element in jsonDa.EnumerateArray())
            {
                // Use the GetString method to get the JSON string and add it to the list
                jsonStrings.Add(element.ToString());
            }       
            var results = new List<object>();

            // Loop through the list of JSON strings
            foreach (var jsonString in jsonStrings)
            {
                // Parse the JSON string and convert it to an object
                var jsonData = JObject.Parse(jsonString);

                // Convert the JSON data to the specified data types
                var medical = jsonData["medical"].Value<bool>();
                var childdx = jsonData["childdx"].Value<bool>();
                var selfharm = jsonData["selfharm"].Value<bool>();
                var sofas = jsonData["sofas"].Value<int>();
                var clinicalstage = jsonData["clinicalstage"].Value<int>();
                var circadian = jsonData["circadian"].Value<bool>();
                var tripartite = jsonData["tripartite"].Value<int>();
                var psychosis = jsonData["psychosis"].Value<bool>();
                var neet = jsonData["neet"].Value<bool>();

                // Use the SingleOrDefault() method instead of First() to avoid an exception
                // if no matching data is found
                var healthData = _db.HealthData.SingleOrDefault(h => h.Medical == medical &&
                      h.ChildDx == childdx &&
                      h.Selfharm == selfharm &&
                      h.Sofas == sofas &&
                      h.ClinicalStage == clinicalstage &&
                      h.Circadian == circadian &&
                      h.Tripartite == tripartite &&
                      h.Psychosis == psychosis &&
                      h.NEET == neet
                );

                // Check if healthData is null, and add an error message to the results list if it is
                if (healthData == null)
                {
                    results.Add(new
                    {
                        Message = "Error, cannot find data entry using the given parameters"
                    });
                }
                else
                {
                    // Create an anonymous object with the data we want to return
                    var res = new
                    {
                        healthData.Alert,
                        healthData.ChildDx,
                        healthData.Circadian,
                        healthData.ClinicalStage,
                        healthData.Constant,
                        healthData.Down
                    };

                    // Add the anonymous object to the results list
                    results.Add(res);
                }
            }
            var Message = JsonSerializer.Serialize(results);

            // Use the JsonSerializer.Serialize() method to convert the results list to a JSON string
            return Task.FromResult(new HealthDataReply
            {
                Message = JsonSerializer.Serialize(results)
            });
        }
    }
}