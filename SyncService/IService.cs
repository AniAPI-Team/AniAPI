﻿using Commons;
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

        #region ServiceStatus

        private ServicesStatus _serviceStatus;
        public ServicesStatus ServiceStatus
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

        public virtual void Stop(Exception ex = null)
        {
            this.UpdateStatus(ServiceStatusEnum.STOPPED);

            if(ex != null)
            {
                this.Log(ex.Message);

                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    this.Log(ex.StackTrace);
                }
            }

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

            Console.WriteLine($"[{this.ServiceStatus.Name}/{from}/{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")}]: Status to {status}");
        }

        public string GetProgress(double actualValue, double maxValue)
        {
            int progress = (int)((actualValue / maxValue) * 100);

            this.ServiceStatus.Progress = progress;

            ServicesStatus servicesStatus = this.ServiceStatus;
            this._serviceStatusCollection.Edit(ref servicesStatus);

            if (progress < 10)
            {
                return "0" + progress.ToString("F0");
            }

            return progress.ToString("F0");
        }

        public string GetProgressD(double actualValue, double maxValue)
        {
            double progress = (actualValue / maxValue) * 100;

            this.ServiceStatus.Progress = progress;

            ServicesStatus servicesStatus = this.ServiceStatus;
            this._serviceStatusCollection.Edit(ref servicesStatus);

            if (progress < 10)
            {
                return "0" + progress.ToString("F2");
            }

            return progress.ToString("F2");
        }

        public void Log(string message, bool updateStatus = false)
        {
            if (updateStatus)
            {
                this.ServiceStatus.Info = message;

                ServicesStatus servicesStatus = this.ServiceStatus;
                this._serviceStatusCollection.Edit(ref servicesStatus);
            }
            else
            {
                Console.WriteLine($"[{this.ServiceStatus.Name}/{this.ServiceStatus.Status}/{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff")}]: {message}");
            }
        }

        #endregion
    }
}
