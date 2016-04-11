using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortSourceTreeBookmarks
{
  [Serializable]
  public class BookmarkNode : TreeViewNode
  {
    public string Path { get; set; }
    public string RepoType { get; set; }
  }
}
