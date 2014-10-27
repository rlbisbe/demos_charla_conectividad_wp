using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace ClientServer
{
    public sealed partial class MainPage : Page
    {
        private void InitializeFigures()
        {

            TestRectangle.ManipulationDelta += TestRectangle_ManipulationDelta;
            TestRectangle.ManipulationCompleted += TestRectangle_ManipulationCompleted;

            TestRectangleImage.ManipulationDelta += TestRectangle_ManipulationDelta;
            TestRectangleImage.ManipulationCompleted += TestRectangle_ManipulationCompleted;

            transforms.Add(TestRectangle, new TranslateTransform());
            transforms.Add(TestRectangleImage, new TranslateTransform());

            ResetPosition(TestRectangle);
            ResetPosition(TestRectangleImage);
        }

        private void ResetPosition(Border rectangle)
        {
            if (rectangle == TestRectangle)
                ResetPosition(rectangle, 10, 300);

            if (rectangle == TestRectangleImage)
                ResetPosition(rectangle, 240, 300);
        }

        private void ResetPosition(Border rectangle, int x, int y)
        {
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            transforms[rectangle] = new TranslateTransform();
            rectangle.RenderTransform = transforms[rectangle];
        }

        async void TestRectangle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Border rectangle = sender as Border;
            GeneralTransform gt = canvas.TransformToVisual(sender as Border);
            Point currentPos = gt.TransformPoint(new Point(0, 0));

            if (Math.Abs(currentPos.Y) < AnotherArea.Height)
            {
                ResetPosition(sender as Border);
                AnotherArea.Fill = new SolidColorBrush(Colors.Green);

                if (rectangle == TestRectangle)
                {
                    string text = (rectangle.Child as TextBlock).Text;
                    await infoSender.SendMessage(text);
                }
                else if (rectangle == TestRectangleImage)
                {
                    byte[] fileBytes;

                    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/face.jpeg", UriKind.Absolute));

                    using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }

                    await infoSender.SendByteArray(fileBytes);
                }

                ResetPosition(rectangle, 10, 300);

                if (rectangle == TestRectangleImage)
                    ResetPosition(rectangle, 240, 300);
            }
        }

        void TestRectangle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Border rectangle = sender as Border;
            transforms[rectangle].X += e.Delta.Translation.X;
            transforms[rectangle].Y += e.Delta.Translation.Y;

            GeneralTransform gt = canvas.TransformToVisual(rectangle);
            Point currentPos = gt.TransformPoint(new Point(0, 0));

            if (Math.Abs(currentPos.Y) < AnotherArea.Height)
            {
                AnotherArea.Fill = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                AnotherArea.Fill = new SolidColorBrush(Colors.Red);
            }
        }

    }
}
