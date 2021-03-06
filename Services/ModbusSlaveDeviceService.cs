using NModbus;
using NModbus.Serial;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusSerilalProxy.Services
{
   public class ModbusSlaveDeviceService
    {
        private const string PrimarySerialPortName = "COM7";

        public async Task StartModbusSerialRtuSlaveWithCustomStore()
        {
            using (SerialPort slavePort = new SerialPort(PrimarySerialPortName))
            {
                // configure serial port
                slavePort.BaudRate = 9600;
                slavePort.DataBits = 8;
                slavePort.Parity = Parity.None;
                slavePort.StopBits = StopBits.One;
                slavePort.Open();

                var factory = new ModbusFactory();
                var slaveNetwork = factory.CreateRtuSlaveNetwork(slavePort);
               
                slavePort.ReadTimeout = 500;
                slavePort.WriteTimeout = 500;
                
                var dataStore = new SlaveStorage();
                var dataStore2 = new SlaveStorage();

                dataStore.HoldingRegisters.StorageOperationOccurred += HoldingRegisters_StorageOperationOccurred;

                IModbusSlave slave1 = factory.CreateSlave(1, dataStore);
                IModbusSlave slave2 = factory.CreateSlave(2, dataStore2);

                slaveNetwork.AddSlave(slave1);
                slaveNetwork.AddSlave(slave2);
              //  await slaveNetwork.ListenAsync();
                await slaveNetwork.ListenWorkAroundAsync(new CancellationToken());

            }
        }

        private void HoldingRegisters_StorageOperationOccurred(object sender, StorageEventArgs<ushort> e)
        {
            Debug.WriteLine($"Holding registers: {e.Operation} starting at {e.StartingAddress}");
        }
    }

    public static class ModbusSlaveDevice
    {
        //https://github.com/NModbus/NModbus/issues/71
        public static async Task ListenWorkAroundAsync(this IModbusSlaveNetwork modbusSlaveNetwork, CancellationToken cancellationToken)
        {
            cancellationToken.Register(modbusSlaveNetwork.Dispose);
            Action listen = () =>
            {
                try
                {
                    modbusSlaveNetwork.ListenAsync(cancellationToken);
                }
                catch (NullReferenceException)
                {
                    System.Diagnostics.Debug.WriteLine("Assuming shutdown of serial port.");
                }
            };

            Task listenCompleteTask = Task.Factory.StartNew(listen, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            await listenCompleteTask.ConfigureAwait(false);
        }

    }
}
