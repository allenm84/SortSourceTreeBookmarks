using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SortSourceTreeBookmarks
{
  [Serializable]
  [XmlInclude(typeof(BookmarkNode))]
  [XmlInclude(typeof(BookmarkFolderNode))]
  public abstract class TreeViewNode
  {
    public int Level { get; set; }
    public bool IsExpanded { get; set; }
    public bool IsLeaf { get; set; }
    public string Name { get; set; }
    public List<TreeViewNode> Children { get; set; }
  }
}
