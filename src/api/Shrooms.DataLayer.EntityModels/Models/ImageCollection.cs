using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class ImageCollection : Collection<string>
    {
        public void Add(ICollection<string> images)
        {
            foreach (var image in images)
            {
                Add(image);
            }
        }

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

                var jsonData = JsonConvert.DeserializeObject<List<string>>(value);
                Items.Clear();
                Add(jsonData);
            }
        }
    }
}
