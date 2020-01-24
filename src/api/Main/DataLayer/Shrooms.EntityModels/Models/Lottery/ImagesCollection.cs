using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Shrooms.EntityModels.Models.Lottery
{
    public class ImagesCollection : Collection<string>
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
            get
            {
                return JsonConvert.SerializeObject(this);
            }
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
