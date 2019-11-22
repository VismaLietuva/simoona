using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Shrooms.EntityModels.Models
{
    public class OfficesCollection : Collection<int>
    {
        public void Add(ICollection<int> offices)
        {
            foreach (var office in offices)
            {
                Add(office);
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

                var jsonData = JsonConvert.DeserializeObject<List<int>>(value);
                Items.Clear();
                Add(jsonData);
            }
        }
    }
}
