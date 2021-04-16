using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace DragDropReunion055Test
{
  
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            fillTree1();
        }
        private void fillTree1()
        {
            TreeViewNode node = new TreeViewNode { Content = "Root", IsExpanded = true };
            treeView1.RootNodes.Add(node);
        }

        private void grid_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                e.AcceptedOperation = DataPackageOperation.Copy;

                var a = e.DataView as DataPackageView;

                if (e.DataView.Contains(StandardDataFormats.Text))
                {
                    e.DragUIOverride.Caption = "⭳ Label";
                }
                else
                {
                    e.DragUIOverride.Caption = "Open";
                }
            }
            catch (Exception ex)
            {
                // Issue 1: Exception on using DataView when contents is set by the app.
                // Set breakpoint and inspect the exception to see the issue
                Debug.WriteLine(ex.Message);
            }
        }

        private async void grid_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    if (items.Count > 0)
                    {
                        List<StorageFile> filesToOpen = new List<StorageFile>();
                        for (int i = 0; i < items.Count; i++)
                        {
                            var storageFile = items[i] as StorageFile;
                            string filePath = storageFile.Path;

                            TreeViewNode node = new TreeViewNode { Content = filePath };
                            treeView1.RootNodes[0].Children.Add(node);
                        }
                    }
                }
                else if (e.DataView.Contains(StandardDataFormats.Text))
                {
                    var text = await e.DataView.GetTextAsync();
                    var node = new TreeViewNode { Content = text };
                    treeView1.RootNodes[0].Children.Add(node);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void OnDragStarting(UIElement sender, DragStartingEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                if (!(sender is TextBlock tbl))
                {
                    args.Cancel = true;
                    return;
                }

                args.AllowedOperations = DataPackageOperation.Copy;
                args.Data.SetData(StandardDataFormats.Text, tbl.Text);

                var bitmap = await GetDragBitmap(tbl);
                if (bitmap != null)
                {
                    // Issue 2 : Setting the bitmap of the DragUI is not working
                    args.DragUI.SetContentFromSoftwareBitmap(bitmap, new Point(6, 6));
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                deferral.Complete();
            }


        }


        public static async Task<SoftwareBitmap> GetDragBitmap(FrameworkElement element)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(element, (int)element.ActualWidth, (int)element.ActualHeight);
            var buffer = await renderTargetBitmap.GetPixelsAsync();

            var bitmap = SoftwareBitmap.CreateCopyFromBuffer(buffer,
                BitmapPixelFormat.Bgra8,
                renderTargetBitmap.PixelWidth,
                renderTargetBitmap.PixelHeight,
                BitmapAlphaMode.Premultiplied);

            return bitmap;
        }
    }
}
