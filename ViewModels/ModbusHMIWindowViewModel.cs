using Microsoft.EntityFrameworkCore;
using ModbusSerilalProxy.Models;
using ModbusSerilalProxy.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusSerilalProxy.ViewModels
{
    class ModbusHMIWindowViewModel : ViewModelBase
    {
        private RelayCommand start;
        private ModbusService _modbusService;
        private HmiDBContext _dbContext;
        private ModbusSlaveDeviceService _modbusSlaveDevice1;

        public ModbusHMIWindowViewModel()
        {
            _modbusService = new ModbusService();
            _modbusService.Device2ValuesRefreshed += _modbusService_Device2ValuesRefreshed;
            _modbusService.ValuesRefreshed += _modbusService_ValuesRefreshed;
            _dbContext = new HmiDBContext();
            _modbusSlaveDevice1 = new ModbusSlaveDeviceService();
        }

     

        /// <summary>
        /// Add new records into database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void _modbusService_Device2ValuesRefreshed(object sender, DeviceEventArgs e)
        {
            _dbContext.Reports.Add(new Report() { RegisterAddress = e.RegisterAddress, Value = e.Value, DateTime = e.DateTime });
            await _dbContext.SaveChangesAsync();
            Reports =await _dbContext.Reports.OrderByDescending((r) => r.DateTime).Take(10).ToListAsync();
        }
        private void _modbusService_ValuesRefreshed(object sender, EventArgs e)
        {
            ConnectionState = _modbusService.ConnectionState;
        }

        public ushort RegisterAddressSlave1 { get; set; }
        public ushort RegisterValueSlave1 { get; set; }
   
        private List<Report> reports;
        public List<Report> Reports
        {
            get { return reports; }
            set { 
                reports = value;
                OnPropertyChanged(nameof(Reports));
            }
        }

        private ConnectionStates connectionStates;
        public ConnectionStates ConnectionState
        {
            get { return connectionStates; }
            set
            {
                connectionStates = value;
                OnPropertyChanged(nameof(ConnectionState));
            }
        }

        /// <summary>
        ///Command to Start modbus service
        /// </summary>
        public RelayCommand Start
        {
            get
            {
                return start ?? (start = new RelayCommand(async(o) =>
              {
                  StartDataExchange();
                  await _modbusSlaveDevice1.StartModbusSerialRtuSlaveWithCustomStore();

              }
              ));
            }
        }
        private void StartDataExchange()
        {
            try
            {
                Task.Run(() => _modbusService.Start());

            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// // Command to put data to register of slave device1 
        /// </summary>
        private RelayCommand putToSlave1;
        public RelayCommand PutToSlave1
        {
            get
            {
                return putToSlave1 ?? (putToSlave1 = new RelayCommand((o) =>
                {
                    
                    _modbusService.RegisterAddressSlave = RegisterAddressSlave1;
                    _modbusService.RegisterValueSlave = RegisterValueSlave1;
                    _modbusService.ModbusSerialRtuMasterWriteRegisters(1, RegisterAddressSlave1, RegisterValueSlave1);
                }
              ));
            }

        }

    }
}
