// self-study exercises derived from Microsoft technical documentation
// https://msdn.microsoft.com/en-us/windows/hardware/drivers/gpio/gpio-interrupts
// baqwas@gmail.com
// armw
// 2017-02-22
// issue: Ich weiß nicht wie es funktioniert :(
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;


namespace PIRCsHeadlessTest
{
    public sealed class StartupTask : IBackgroundTask
    {
        private const int pinPIR = 6;   // BCM convention: GPIO6 = Board pin 31 on jumper J8
        private const int pinLED = 26;  // BCM convention: GPIO26 = Board pin 37 on jumper J8
        private BackgroundTaskDeferral myDeferral;
        private GpioPin pinUnderTest;   // motion sensor data pin
        private GpioPin pinBlink;       // LED for visual feedback
        private static GpioPinDriveMode pinMode = GpioPinDriveMode.Input;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            myDeferral = taskInstance.GetDeferral();
            var gpioController = Windows.Devices.Gpio.GpioController.GetDefault();
            if (gpioController != null)
            {
                pinBlink = gpioController.OpenPin(pinLED);
                pinBlink.SetDriveMode(GpioPinDriveMode.Output);
                BlinkMe(pinBlink, 1);
                int pinCount = gpioController.PinCount;
                System.Diagnostics.Debug.WriteLine("Board has " + pinCount + " pins");
                BlinkMe(pinBlink, 2);
                if (pinPIR <= pinCount)
                {
                    System.Diagnostics.Debug.WriteLine("Assigning pin " + pinPIR + " for tests");
                    BlinkMe(pinBlink, 4);
                    pinUnderTest = gpioController.OpenPin(pinPIR);
                    if (pinUnderTest != null)
                    {
                        pinUnderTest.DebounceTimeout = TimeSpan.FromMilliseconds(100);  // 100 ms, nominal value
                        pinUnderTest.SetDriveMode(pinMode);                             // pin data to be read by app
                        pinUnderTest.ValueChanged += pinUnderTest_ValueChanged;         // event handler for rising/falling signal
                        System.Diagnostics.Debug.WriteLine("Awaiting state change for pin " + pinPIR +
                            " in mode " + pinMode.ToString());
                        BlinkMe(pinBlink, 8);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Unable to initialize GPIO pin " + pinPIR);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Pin number under test " + pinPIR + " beyond upper limit of " + pinCount);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Unable to access GPIO controller");
            }
        }

        private void pinUnderTest_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Pin " + pinPIR + " state is " + args.Edge.ToString());
        }

        private void BlinkMe(GpioPin pin, int seconds)
        {
            int i = 0;
            const int one_second = 1000;

            for (i = 0; i < seconds; i++)
            {
                pin.Write(GpioPinValue.High);
                Task.Delay(-1).Wait(one_second);
                pin.Write(GpioPinValue.Low);
                Task.Delay(-1).Wait(one_second);
            }
        } // seriously?
    }
}
