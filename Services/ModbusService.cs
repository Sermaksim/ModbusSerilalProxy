using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ModbusSerilalProxy.Models;
using NModbus;
using NModbus.Serial;
using NModbus.Utility;


namespace ModbusSerilalProxy.Services
{
    class ModbusService
    {
        private const string PrimarySerialPortName = "COM12";
        private readonly System.Timers.Timer _timer;
        private DateTime _lastScanTime;
        public event EventHandler ValuesRefreshed;
        public event EventHandler<DeviceEventArgs> Device2ValuesRefreshed;
        private volatile object _locker = new object();

       private byte device1SlaveID = 1;
       private byte device2SlaveID = 2;
        // configure serial port
        private int _baudRate = 9600;
        int _dataBits = 8;
        Parity _parity = Parity.None;
        StopBits _stopBits = StopBits.One;

        public ushort RegisterAddressSlave { get; set; }
        public ushort RegisterValueSlave { get; set; }
        public ConnectionStates ConnectionState { get; private set; }

        public ModbusService()
        {
            _timer = new System.Timers.Timer();
            _timer.Interval = 500;
            _timer.Elapsed += OnTimerElapsed;
        }
        public void Start()
        {
            try
            {
                var currentThread = Thread.CurrentThread.ManagedThreadId;
                _timer.Start();
                ConnectionState = ConnectionStates.Online;
                OnValuesRefreshed();
            }
            catch
            {
                ConnectionState = ConnectionStates.Offline;
                OnValuesRefreshed();
                throw;
            }
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _timer.Stop();
                try
                {
                    RefreshValues();

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                OnValuesRefreshed();

            }
            finally
            {
                _timer.Start();
            }
            _lastScanTime = DateTime.Now;
        }

        private void RefreshValues()
        {

            lock (_locker)
            {

                try
                {
                    var recivedValueFromDevice1 = ModbusSerialRtuMasterReadRegisters(RegisterAddressSlave, 1, device1SlaveID);
                    var recivedValueFromDevice2 = ModbusSerialRtuMasterReadRegisters(RegisterAddressSlave, 1, device2SlaveID);
                    if (recivedValueFromDevice1 != recivedValueFromDevice2)
                    {
                        ModbusSerialRtuMasterWriteRegisters(device2SlaveID, RegisterAddressSlave, recivedValueFromDevice1);
                        recivedValueFromDevice2 = ModbusSerialRtuMasterReadRegisters(RegisterAddressSlave, 1, device2SlaveID);
                        Device2ValuesRefreshed?.Invoke(this, new DeviceEventArgs(RegisterAddressSlave, recivedValueFromDevice2, DateTime.Now));
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        

        private void OnValuesRefreshed()
        {
            ValuesRefreshed?.Invoke(this, new EventArgs());
        }

       

        /// <summary>
        ///      Modbus serial RTU master write holding registers 
        /// </summary>
        public void ModbusSerialRtuMasterWriteRegisters(byte slaveId, ushort startAddress, ushort registerValue)
        {

            lock (_locker)
            {
                using (SerialPort port = new SerialPort(PrimarySerialPortName))
                {
                    // configure serial port
                    port.BaudRate = _baudRate;
                    port.DataBits = _dataBits;
                    port.Parity = _parity;
                    port.StopBits = _stopBits;
                    port.Open();

                    var factory = new ModbusFactory();
                    IModbusMaster master = factory.CreateRtuMaster(port);

                    byte _slaveId = slaveId;
                    ushort _startAddress = startAddress;

                    // write three registers
                  
                    master.WriteSingleRegister(_slaveId, _startAddress, registerValue);


                }
            }
        }

        public ushort ModbusSerialRtuMasterReadRegisters(ushort startAddress=1, ushort numRegisters = 5, byte slaveId=1)
        {
            using (SerialPort port = new SerialPort(PrimarySerialPortName))
            {
                // configure serial port
                port.BaudRate = _baudRate;
                port.DataBits = _dataBits;
                port.Parity = _parity;
                port.StopBits = _stopBits;
                port.Open();
                var t = port.ReadTimeout;

                var factory = new ModbusFactory();
                IModbusSerialMaster master = factory.CreateRtuMaster(port);
                master.Transport.ReadTimeout = 1000;

                byte _slaveId = slaveId;
                ushort _startAddress = startAddress;//1;
                ushort _numRegisters = numRegisters;

                // read five registers		
                ushort[] registers = master.ReadHoldingRegisters(_slaveId, _startAddress, _numRegisters);

                var currentThread = Thread.CurrentThread.ManagedThreadId;
                port.Close();
                return registers[0];
            }

            // output: 
            // Register 1=0
            // Register 2=0
            // Register 3=0
            // Register 4=0
            // Register 5=0
        }

    }
}
