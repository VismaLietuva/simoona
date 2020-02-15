using Newtonsoft.Json;

namespace Shrooms.DataLayer.EntityModels.Models.Notifications
{
    public class Sources
    {
        public int PostId { get; set; }

        public string EventId { get; set; }

        public string ProjectId { get; set; }

        public int WallId { get; set; }

        [JsonIgnore]
        public string Serialized
        {
            get => JsonConvert.SerializeObject(this);

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                var jsonData = JsonConvert.DeserializeObject<Sources>(value);
                PostId = jsonData.PostId;
                EventId = jsonData.EventId;
                ProjectId = jsonData.ProjectId;
                WallId = jsonData.WallId;
            }
        }
    }
}
