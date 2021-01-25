using Commons;
using Commons.Collections;
using Commons.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SyncService
{
    public abstract class IService
    {
        #region Members

        protected ServicesStatusCollection _serviceStatusCollection = new ServicesStatusCollection();
        protected ServicesLogCollection _ServicesLogCollection = new ServicesLogCollection();

        #region ServiceStatus

        private ServicesStatus _serviceStatus;
        protected ServicesStatus ServiceStatus
        {
            get
            {
                if(this._serviceStatus == null)
                {
                    this._serviceStatus = this.GetServiceStatus();
                }

                return this._serviceStatus;
            }
            set
            {
                this._serviceStatus = value;
            }
        }
        protected abstract ServicesStatus GetServiceStatus();

        #endregion

        protected abstract int TimeToWait { get; }

        #endregion

        #region Methods

        public virtual void Start()
        {
            this.UpdateStatus(ServiceStatusEnum.STARTING);

            this.Work();
        }

        public virtual void Work()
        {
            this.UpdateStatus(ServiceStatusEnum.WORKING);
        }

        public virtual void Wait()
        {
            this.UpdateStatus(ServiceStatusEnum.WAITING);

            Thread.Sleep(this.TimeToWait);
            
            this.Work();
        }

        public virtual void Stop()
        {
            this.UpdateStatus(ServiceStatusEnum.STOPPED);

            Thread.Sleep(60 * 1000);

            this.Start();
        }

        protected void UpdateStatus(ServiceStatusEnum status)
        {
            ServiceStatusEnum from = this.ServiceStatus.Status;
            this.ServiceStatus.Status = status;

            ServicesStatus servicesStatus = this.ServiceStatus;

            if(this._serviceStatusCollection.Exists(ref servicesStatus))
            {
                this._serviceStatusCollection.Edit(ref servicesStatus);
            }
            else
            {
                this._serviceStatusCollection.Add(ref servicesStatus);
            }

            Console.WriteLine($"[{this.ServiceStatus.Name}/{from}/{DateTime.Now.ToString("dd-MM-yyyy HH:mm.ms")}]: Status to {status}");
        }

        protected string GetProgress(double actualValue, double maxValue)
        {
            double progress = (actualValue / maxValue) * 100;

            if (progress < 10)
            {
                return "0" + progress.ToString("F0");
            }

            return progress.ToString("F0");
        }

        protected void Log(string message)
        {
            ServicesLog log = new ServicesLog()
            {
                ServiceId = this.ServiceStatus.Id,
                Message = message
            };
            this._ServicesLogCollection.Add(ref log);
            
            Console.WriteLine($"[{this.ServiceStatus.Name}/{this.ServiceStatus.Status}/{DateTime.Now.ToString("dd-MM-yyyy HH:mm.ms")}]: {message}");
        }

        #endregion
    }
}
