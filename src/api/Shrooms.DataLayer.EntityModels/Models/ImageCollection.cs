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

                if (!IsJsonArray(value))
                {
                    Add(value);
                    return;
                }

                var jsonData = JsonConvert.DeserializeObject<List<string>>(value);

                Items.Clear();
                
                Add(jsonData);
            }
        }

        private bool IsJsonArray(string value)
        {
            return value.StartsWith("[") && value.EndsWith("]");
        }

        private void AddImages(IEnumerable<string> images)
        {
            if (images == null)
            {
                return;
            }

            foreach (var image in images)
            {
                if (string.IsNullOrEmpty(image))
                {
                    continue;
                }

                Add(image);
            }
        }
    }
}
