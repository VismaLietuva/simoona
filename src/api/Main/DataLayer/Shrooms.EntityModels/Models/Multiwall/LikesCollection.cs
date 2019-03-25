using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Shrooms.EntityModels.Models.Multiwall
{
    public class LikesCollection : Collection<Like>
    {
        public void Add(ICollection<Like> likes)
        {
            foreach (var like in likes)
            {
                Add(like);
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

                var jsonData = JsonConvert.DeserializeObject<List<Like>>(value);
                Items.Clear();
                Add(jsonData);
            }
        }
    }
}
