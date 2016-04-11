using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SortSourceTreeBookmarks
{
  public class SortSourceTreeAppContext : ApplicationContext
  {
    static readonly XmlSerializer ser;
    static SortSourceTreeAppContext()
    {
      ser = new XmlSerializer(typeof(List<TreeViewNode>));
    }

    public SortSourceTreeAppContext()
    {
      PerformSort();
    }

    private void Close()
    {
      Application.Exit();
    }

    private async void PerformSort()
    {
      await Task.Yield();

      string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

      string sourceTree = Path.Combine(appData, @"Atlassian\SourceTree");
      if (!Directory.Exists(sourceTree))
      {
        ShowError("Unable to locate {0}. Make sure SourceTree is installed.", sourceTree);
        return;
      }

      string bookmarks = Path.Combine(sourceTree, "bookmarks.xml");
      if (!File.Exists(bookmarks))
      {
        ShowError("Unable to locate {0}. Did you create bookmarks?", bookmarks);
        return;
      }

      var processes = Process.GetProcessesByName("sourcetree");
      if (processes.Length > 0)
      {
        var result = MessageBox.Show("SourceTree is running. Would you like to close it? Selecting 'No' will cancel the sort.", "Confirm",
          MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.No)
        {
          Close();
          return;
        }

        var wait = TimeSpan.FromMinutes(5);
        foreach (var proc in processes)
        {
          try
          {
            proc.CloseMainWindow();
            if (!proc.WaitForExit(wait.Milliseconds))
            {
              throw new TimeoutException(string.Format("Exceeded the {0} timeout", wait));
            }
          }
          catch (Exception ex)
          {
            ShowError("Unable to close SourceTree because: {0}", ex.Message);
            return;
          }
        }
      }

      List<TreeViewNode> nodes;
      try
      {
        using (var stream = File.OpenRead(bookmarks))
        {
          nodes = ser.Deserialize(stream) as List<TreeViewNode>;
        }
      }
      catch (Exception ex)
      {
        nodes = null;
        ShowError("Unable to load the bookmarks because: {0}", ex.Message);
        return;
      }

      try
      {
        SortNodes(nodes);
      }
      catch (Exception ex)
      {
        ShowError("Unable to sort nodes because: {0}", ex.Message);
        return;
      }

      try
      {
        File.Move(bookmarks, Path.ChangeExtension(bookmarks, ".xml.bak"));
      }
      catch (Exception ex)
      {
        ShowError("Unable to create backup of existing bookmarks.xml because {0}", ex.Message);
        return;
      }

      try
      {
        using (var stream = File.Create(bookmarks))
        {
          ser.Serialize(stream, nodes);
        }
      }
      catch (Exception ex)
      {
        ShowError("Unable to save bookmarks because: {0}", ex.Message);
        return;
      }

      MessageBox.Show("Sort completed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
      Close();
    }

    private void SortNodes(List<TreeViewNode> nodes)
    {
      if (nodes == null)
      {
        return;
      }

      if (nodes.Count == 0)
      {
        return;
      }

      nodes.Sort((a, b) => a.Name.CompareTo(b.Name));
      nodes.ForEach(n => SortNodes(n.Children));
    }

    private void ShowError(string format, params object[] args)
    {
      ShowError(string.Format(format, args));
    }

    private void ShowError(string text)
    {
      MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      Close();
    }
  }
}