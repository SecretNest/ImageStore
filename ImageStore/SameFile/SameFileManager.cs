using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecretNest.ImageStore.SameFile
{
    partial class SameFileManager : Form
    {
        Dictionary<Guid, SameFileMatchingRecord> records;
        IOrderedEnumerable<KeyValuePair<Guid, SameFileMatchingRecord>> ordered;
        Dictionary<int, List<ListViewItem>> groupedItemsNotInTargetFolder = new Dictionary<int, List<ListViewItem>>();
        Dictionary<int, List<ListViewItem>> groupedItemsInTargetFolder = new Dictionary<int, List<ListViewItem>>();
        Color oddGroupLinesBackColor, evenGroupLinesBackColor, normalBackColor, normalForeColor, selectedForeColor, ignoredForeColor;
        Func<Guid, bool, bool> markIgnoreCallback;
        bool showAuto = false;
        bool preventNonReserved = true;
        bool showDifferentBackColor = true;

        bool listLoading = false;

        internal SameFileManager(Dictionary<Guid, SameFileMatchingRecord> records,
            Color oddGroupLinesBackColor, Color evenGroupLinesBackColor, Color normalBackColor,
            Color normalForeColor, Color selectedForeColor, Color ignoredForeColor,
            Func<Guid, bool, bool> markIgnoreCallback)
        {
            InitializeComponent();
            this.records = records;
            this.oddGroupLinesBackColor = oddGroupLinesBackColor;
            this.evenGroupLinesBackColor = evenGroupLinesBackColor;
            this.normalBackColor = normalBackColor;
            this.normalForeColor = normalForeColor;
            this.selectedForeColor = selectedForeColor;
            this.ignoredForeColor = ignoredForeColor;
            this.markIgnoreCallback = markIgnoreCallback;
        }

        private void SameFileManager_Load(object sender, EventArgs e)
        {
            ordered = records.OrderBy(i => i.Value.GroupNumber).ThenBy(i => i.Value.FileName);
            LoadToList();
            TopMost = true;
            Focus();
            TopMost = false;
        }

        int lastOrder = 1;
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == lastOrder)
                return;
            else
                lastOrder = e.Column;
            switch (e.Column)
            {
                case 0: //checked
                    showDifferentBackColor = false;
                    ordered = records.OrderByDescending(i => i.Value.IsSelectedToReturn).ThenBy(i => i.Value.GroupNumber).ThenByDescending(i => i.Value.IsInTargetFolder).ThenBy(i => i.Value.FileName);
                    break;
                case 1: //group
                    showDifferentBackColor = true;
                    ordered = records.OrderBy(i => i.Value.GroupNumber).ThenByDescending(i => i.Value.IsInTargetFolder).ThenBy(i => i.Value.FileName);
                    break;
                case 2: //filename
                case 3:
                    showDifferentBackColor = false;
                    ordered = records.OrderByDescending(i => i.Value.IsInTargetFolder).ThenBy(i => i.Value.FileName).ThenBy(i => i.Value.GroupNumber);
                    break;
                case 4: //ignored
                    showDifferentBackColor = false;
                    ordered = records.OrderByDescending(i => i.Value.IsIgnored).ThenBy(i => i.Value.GroupNumber).ThenByDescending(i => i.Value.IsInTargetFolder).ThenBy(i => i.Value.FileName);
                    break;
                case 5: //auto
                    showDifferentBackColor = true;
                    ordered = records.OrderByDescending(i => i.Value.IsAutoDealt).ThenBy(i => i.Value.GroupNumber).ThenByDescending(i => i.Value.IsInTargetFolder).ThenBy(i => i.Value.FileName);
                    break;
            }
            LoadToList();
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (listLoading) return;

            var id = (Guid)e.Item.Tag;
            var record = records[id];
            var check = e.Item.Checked;

            if (check)
            {
                if (!record.IsInTargetFolder || record.IsIgnored)
                {
                    //Not in target folder => must not return, must select
                    //Ignored item => must not return, must select

                    listLoading = true;
                    e.Item.Checked = false;
                    listLoading = false;
                    e.Item.ForeColor = normalForeColor;
                    return;
                }

                //Check all selected
                if (preventNonReserved)
                {
                    //if there is any file in other folder and not set to ignored, all selected is also allowed.
                    if (!groupedItemsNotInTargetFolder.ContainsKey(record.GroupNumber) || !groupedItemsNotInTargetFolder[record.GroupNumber].Where(i => !records[(Guid)i.Tag].IsIgnored).Any())
                    {
                        var group = groupedItemsInTargetFolder[record.GroupNumber]
                            .Select(i => new { ListViewItem = i, Record = records[(Guid)i.Tag] })
                            .Where(i => !i.Record.IsIgnored);
                        var groupCount = group.Count();
                        if (groupCount <= 1)
                        {
                            listLoading = true;
                            e.Item.Checked = false;
                            e.Item.ForeColor = normalForeColor;
                            listLoading = false;
                            return;
                        }
                        else if (groupCount == 2)
                        {
                            var another = group.Where(i => i.ListViewItem != e.Item).First();
                            if (another.ListViewItem.Checked)
                            {
                                listLoading = true;
                                another.ListViewItem.Checked = false;
                                listLoading = false;
                                another.ListViewItem.ForeColor = normalForeColor;
                                another.Record.IsSelectedToReturn = false;
                            }
                        }
                        else //groupCount >= 3
                        {
                            var others = group.Where(i => i.ListViewItem != e.Item);
                            if (!others.Any(i => !i.Record.IsSelectedToReturn))
                            {
                                var one = others.First();
                                listLoading = true;
                                one.ListViewItem.Checked = false;
                                listLoading = false;
                                one.ListViewItem.ForeColor = normalForeColor;
                                one.Record.IsSelectedToReturn = false;
                            }
                        }
                    }
                }

                record.IsSelectedToReturn = true;
                e.Item.ForeColor = selectedForeColor;
            }
            else
            {
                record.IsSelectedToReturn = false;
                e.Item.ForeColor = normalForeColor;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            preventNonReserved = checkBox2.Checked;
            if (preventNonReserved)
            {
                foreach (var group in groupedItemsInTargetFolder)
                {
                    //if there is any file in other folder and not set to ignored, all selected is also allowed.
                    if (!groupedItemsNotInTargetFolder.ContainsKey(group.Key) || !groupedItemsNotInTargetFolder[group.Key].Where(i => !records[(Guid)i.Tag].IsIgnored).Any())
                    {
                        var inFolder = groupedItemsInTargetFolder[group.Key]
                            .Select(i => new { ListViewItem = i, Record = records[(Guid)i.Tag] })
                            .Where(i => !i.Record.IsIgnored);
                        var groupCount = inFolder.Count();
                        if (groupCount == 1)
                        {
                            var one = inFolder.First();
                            if (one.Record.IsSelectedToReturn)
                            {
                                listLoading = true;
                                one.ListViewItem.Checked = false;
                                listLoading = false;
                                one.ListViewItem.ForeColor = normalForeColor;
                                one.Record.IsSelectedToReturn = false;
                            }
                        }
                        else if (groupCount >= 2)
                        {
                            if (!inFolder.Any(i => !i.Record.IsSelectedToReturn))
                            {
                                var one = inFolder.First();
                                if (one.Record.IsSelectedToReturn)
                                {
                                    listLoading = true;
                                    one.ListViewItem.Checked = false;
                                    listLoading = false;
                                    one.ListViewItem.ForeColor = normalForeColor;
                                    one.Record.IsSelectedToReturn = false;
                                }
                            }
                        }
                        //groupCount = 0: Do nothing
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var group in groupedItemsInTargetFolder)
            {
                if (groupedItemsNotInTargetFolder.ContainsKey(group.Key) && groupedItemsNotInTargetFolder[group.Key].Where(i => !records[(Guid)i.Tag].IsIgnored).Any())
                {
                    //if there is any file in other folder and not set to ignored, all selected is also allowed.
                    foreach (var item in group.Value)
                    {
                        if (!item.Checked)
                        {
                            var record = records[(Guid)item.Tag];
                            if (!record.IsIgnored)
                            {
                                listLoading = true;
                                item.Checked = true;
                                listLoading = false;
                                item.ForeColor = selectedForeColor;
                                record.IsSelectedToReturn = true;
                            }
                        }
                    }
                }
                else
                {
                    //select all but one
                    var notIgnored = group.Value.Select(i => new { ListViewItem = i, Record = records[(Guid)i.Tag] }).Where(i => !i.Record.IsIgnored);
                    var notIgnoredCount = notIgnored.Count();
                    if (notIgnoredCount == 1)
                    {
                        var one = notIgnored.First();
                        if (one.Record.IsSelectedToReturn)
                        {
                            listLoading = true;
                            one.ListViewItem.Checked = false;
                            listLoading = false;
                            one.ListViewItem.ForeColor = normalForeColor;
                            one.Record.IsSelectedToReturn = false;
                        }
                    }
                    else if (notIgnoredCount >= 2)
                    {
                        var notSelected = notIgnored.Where(i => !i.Record.IsSelectedToReturn);
                        var notSelectedCount = notSelected.Count();
                        if (notSelectedCount == 0)
                        {
                            //Uncheck the 1st one if all are selected
                            var one = notIgnored.First();
                            listLoading = true;
                            one.ListViewItem.Checked = false;
                            listLoading = false;
                            one.ListViewItem.ForeColor = normalForeColor;
                            one.Record.IsSelectedToReturn = false;
                        }
                        else if (notSelectedCount >= 2)
                        {
                            //Select all but the 1st.
                            listLoading = true;
                            foreach (var one in notSelected.Skip(1))
                            {
                                one.ListViewItem.Checked = true;
                                one.ListViewItem.ForeColor = selectedForeColor;
                                one.Record.IsSelectedToReturn = true;
                            }
                            listLoading = false;
                        }
                        //notSelectedCount = 1: Do nothing
                    }
                    //notIgnoredCount = 0: Do nothing
                }
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Selected)
                {
                    var filename = item.SubItems[2].Text;
                    System.Diagnostics.Process.Start(filename);
                    return;
                }
            }
        }

        bool mouseDown = false;
        private void listView1_MouseLeave(object sender, EventArgs e)
        {
            mouseDown = false;
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
        }

        private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (mouseDown && !listLoading)
            {
                e.NewValue = e.CurrentValue;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Application.DoEvents();
            showAuto = !checkBox1.Checked;
            LoadToList();
        }

        void LoadToList()
        {
            listLoading = true;
            listView1.Items.Clear();
            groupedItemsInTargetFolder.Clear();
            groupedItemsNotInTargetFolder.Clear();
            //listView1.SuspendLayout();

            int lastGroupNumber = 0;
            bool oddGroup = false;
            foreach (var record in ordered)
            {
                if (record.Value.IsAutoDealt && !showAuto) continue;
                ListViewItem item = new ListViewItem(new string[]
                {
                    "",
                    record.Value.GroupNumber.ToString(),
                    record.Value.FileName,
                    record.Value.IsInTargetFolder ? "Yes" : "",
                    record.Value.IsIgnored ? "Ignored" : "",
                    record.Value.IsAutoDealt ? "Auto Dealt" : ""
                })
                {
                    Checked = record.Value.IsSelectedToReturn,
                    Tag = record.Key,
                    ForeColor = record.Value.IsIgnored ? ignoredForeColor : (record.Value.IsSelectedToReturn ? selectedForeColor : normalForeColor)
                };
                if (showDifferentBackColor)
                {
                    if (lastGroupNumber != record.Value.GroupNumber)
                    {
                        lastGroupNumber = record.Value.GroupNumber;
                        oddGroup = !oddGroup;
                    }
                    item.BackColor = oddGroup ? oddGroupLinesBackColor : evenGroupLinesBackColor;
                }
                else
                {
                    item.BackColor = normalBackColor;
                }

                listView1.Items.Add(item);
                if (record.Value.IsInTargetFolder)
                {
                    if (!groupedItemsInTargetFolder.TryGetValue(record.Value.GroupNumber, out var items))
                    {
                        items = new List<ListViewItem>();
                        groupedItemsInTargetFolder.Add(record.Value.GroupNumber, items);
                    }
                    items.Add(item);
                }
                else
                {
                    if (!groupedItemsNotInTargetFolder.TryGetValue(record.Value.GroupNumber, out var items))
                    {
                        items = new List<ListViewItem>();
                        groupedItemsNotInTargetFolder.Add(record.Value.GroupNumber, items);
                    }
                    items.Add(item);
                }
            }
            listLoading = false;

            if (showAuto)
            {
                listView1.Columns[5].Width = 80;
            }
            else
            {
                listView1.Columns[5].Width = 0;
            }

            //listView1.ResumeLayout();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MarkIgnore(true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MarkIgnore(false);
        }

        void MarkIgnore(bool state)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Selected)
                {
                    var id = (Guid)item.Tag;
                    if (markIgnoreCallback(id, state))
                    {
                        var record = records[id];
                        record.IsIgnored = state;
                        if (state)
                        {
                            record.IsSelectedToReturn = false;
                            listLoading = true;
                            item.Checked = false;
                            listLoading = false;
                            item.ForeColor = ignoredForeColor;
                            item.SubItems[4].Text = "Ignored";
                        }
                        else
                        {
                            item.SubItems[4].Text = "";
                            item.Checked = true; //trigger the event for checked checking.
                        }
                    }
                }
            }
        }
    }
}