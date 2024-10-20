using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace GrafanaZendeskIntegration.FunctionApp.Services
{
    public class ZendeskTicketResponseConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return typeof(ZendeskTicketResponse).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            ZendeskTicketResponse result = null;

            if (jsonObject["ticket"] != null)
            {
                result = new ZendeskTicket();
            }
            else if (jsonObject["description"] != null)
            {
                result = new SimpleError();
            }
            else if (jsonObject["error"]?["title"] != null)
            {
                result = new DetailedError();
            }

            if (result != null)
            {
                serializer.Populate(jsonObject.CreateReader(), result);
            }
            else
            {
                throw new JsonSerializationException("Unknown schema");
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Implement if needed. For deserialization only, this can be left empty or throw an exception.  
            throw new NotImplementedException();
        }
    }
}
