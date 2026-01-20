using SosyalAjandam.Models;

namespace SosyalAjandam.ViewModels
{
    public class GroupIndexViewModel
    {
        public IEnumerable<Group> MyGroups { get; set; } = new List<Group>();
        public IEnumerable<Group> DiscoverGroups { get; set; } = new List<Group>();
        public string SearchString { get; set; } = string.Empty;
    }
}
