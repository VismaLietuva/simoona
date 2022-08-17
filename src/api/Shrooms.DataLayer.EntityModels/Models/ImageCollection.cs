using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class ImageCollection : Collection<string>
    {
        public ImageCollection()
        {
        }

        public ImageCollection(IEnumerable<string> images)
        {
            AddImages(images);
        }

        public void Add(ICollection<string> images)
        {
            AddImages(images);
        }

        private void AddImages(IEnumerable<string> images)
        {
            foreach (var image in images)
            {
                if (string.IsNullOrEmpty(image))
                {
                    continue;
                }

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
