// Copyright (c) Microsoft. All rights reserved.

using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using System.Threading;
using System.Diagnostics;

namespace Blinky
{
    public sealed partial class MainPage : Page
    {
        private int CLK_PIN = 17;
        private int R1_PIN = 5;
        private int LAT_PIN = 25;


        private GpioPin clkPin;
        private GpioPin r1Pin;
        private GpioPin r2Pin;
        private GpioPin latPin;

        private GpioPin APin;
        private GpioPin BPin;
        private GpioPin CPin;

        private GpioPin zeroPin;
        private GpioPinValue pinValue;
        private DispatcherTimer timer;

        byte[] bitmap = new byte[512];

        


        private bool _low = false;

        public MainPage()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            InitGPIO();
            for(int height =0; height < 16; ++height)
            {
                for (int width = 0; width < 32; ++width)
                {
                    bitmap[height * 32 + width] = 0;
                }
            }

            
            bitmap[32 * 1 + 1] = 1;
            bitmap[32 * 1 + 2] = 1;
            bitmap[32 * 1 + 3] = 1;

            bitmap[32 * 2 + 1] = 1;
            bitmap[32 * 2 + 3] = 1;


            bitmap[32 * 3 + 1] = 1;
            bitmap[32 * 3 + 2] = 1;
            bitmap[32 * 3 + 3] = 1;

            bitmap[32 * 4 + 1] = 1;
            bitmap[32 * 4 + 3] = 1;
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            clkPin = gpio.OpenPin(CLK_PIN);
            r1Pin = gpio.OpenPin(R1_PIN);
            latPin = gpio.OpenPin(LAT_PIN);
            zeroPin = gpio.OpenPin(27);

            r2Pin = gpio.OpenPin(6);

            APin = gpio.OpenPin(18);
            BPin = gpio.OpenPin(23);
            CPin = gpio.OpenPin(24);

            pinValue = GpioPinValue.High;
            zeroPin.SetDriveMode(GpioPinDriveMode.Output);
            clkPin.SetDriveMode(GpioPinDriveMode.Output);
            r1Pin.SetDriveMode(GpioPinDriveMode.Output);

            r2Pin.SetDriveMode(GpioPinDriveMode.Output);
            latPin.SetDriveMode(GpioPinDriveMode.Output);

            APin.SetDriveMode(GpioPinDriveMode.Output);
            BPin.SetDriveMode(GpioPinDriveMode.Output);
            CPin.SetDriveMode(GpioPinDriveMode.Output);

            zeroPin.Write(GpioPinValue.Low);
            APin.Write(GpioPinValue.Low);
            BPin.Write(GpioPinValue.Low);
            CPin.Write(GpioPinValue.Low);

            GpioStatus.Text = "GPIO pin initialized correctly.";

        }



        private void DrawBitmap()
        {
            for(int rows = 0;rows< 8; ++rows)
            {
                SetRow(rows);
                DrawBitmapRow(rows);
                // draw contents of the row

            }
        }

        private void DrawBitmapRow(int currentRow)
        {
            var start = 32 * currentRow;
            var lowerStart = 32 * (currentRow + 8);
            for (int pixel = 0; pixel< 32; ++pixel)
            {
                SetPixel1Status(bitmap[start + pixel] > 0);
                SetPixel2Status(bitmap[lowerStart + pixel] > 0);

                ClockPixel();
            }
            Latch();
        }

        private void SetPixel1Status(bool value)
        {
            r1Pin.Write(value ? GpioPinValue.High : GpioPinValue.Low);
        }
        private void SetPixel2Status(bool value)
        {
            r2Pin.Write(value ? GpioPinValue.High : GpioPinValue.Low);
        }

        private void Latch()
        {
            latPin.Write(GpioPinValue.High);
            latPin.Write(GpioPinValue.Low);
        }

        private void DisablePixel(int currentRow)
        {
            r1Pin.Write(GpioPinValue.Low);
            r2Pin.Write(GpioPinValue.Low);
        }

        private void ClockPixel()
        {
            clkPin.Write(GpioPinValue.High);
            clkPin.Write(GpioPinValue.Low);
        }

        private void DrawBitmapPixel(int currentRow)
        {
            if (currentRow < 8)
            {
                r1Pin.Write(GpioPinValue.High);
                r2Pin.Write(GpioPinValue.Low);
            }
            else
            {
                r1Pin.Write(GpioPinValue.Low);

                r2Pin.Write(GpioPinValue.High);
            }
        }


        int rowIndex = 0;
        private void DrawScreen()
        {
            //for(int i =0; i< 8;++i)
            //{
            //    DrawRow(i);
            //}
            DrawRow(rowIndex);
            rowIndex++;
            if(rowIndex == 16)
            {
                rowIndex = 0;
            }
        }

        private void DrawRow(int index)
        {
            SetRow(index % 8);
            // Iterate through all the pixels on the row.
            for (int i = 0; i < 32; i++)
            {
                DrawPixel(index);

            }
            latPin.Write(GpioPinValue.High);
            latPin.Write(GpioPinValue.Low);
        }

        private void SetRow(int index)
        {
            var apinSet = index & 0x1;
            var bpinSet = index & 0x2;
            var cpinSet = index & 0x4;

            APin.Write(apinSet > 0 ? GpioPinValue.High : GpioPinValue.Low);

            BPin.Write(bpinSet > 0 ? GpioPinValue.High : GpioPinValue.Low);

            CPin.Write(cpinSet > 0 ? GpioPinValue.High : GpioPinValue.Low);
        }

        private void DrawPixel(int rowNumber)
        {
            // Set CLK to notify matrix of incoming byte
            

            // Set Red-Color on Row.
            if(rowNumber < 8)
            {
                r1Pin.Write(GpioPinValue.High);
                r2Pin.Write(GpioPinValue.Low);
            }
            else
            {
                r1Pin.Write(GpioPinValue.Low);

                r2Pin.Write(GpioPinValue.High);
            }
            clkPin.Write(GpioPinValue.High);
            clkPin.Write(GpioPinValue.Low);

        }


        private void Timer_Tick(object sender, object e)
        {
            _low = !_low;
            // Draw the screen
            //DrawScreen();
            DrawBitmap();
        }
             

    }
}
