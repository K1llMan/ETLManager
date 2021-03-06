﻿using System;
using System.Net;
using System.Threading;

using ETLApp;
using ETLCommon;

using ETLProgramCommon.DataAccess;

using FluentFTP.Proxy;

namespace TestPump
{
    public class TestPump: ETLProgram
    {
        #region Тесты

        private void ArchivesTest()
        {
            Archivator.ExtractAll(RootInDir, RootOutDir);
        }

        private void FtpTest()
        {
            FtpConnector connector = new FtpConnector(new FtpParams {
                Host = "host",
                Credentials = new NetworkCredential("name", "pass"),
                Proxy = Context.Settings.Proxy.As<ProxyInfo>()
            });

            connector.Connect();
            connector.WorkDir = "WorkDir";

            connector.GetFiles(RootOutDir.FullName);
        }

        private void ServiceTest()
        {
            TestWebService service = new TestWebService("http://url.com/api/", Context.Settings.Proxy.As<IWebProxy>());
            string data = service.GetData();
        }

        #endregion Тесты


        public void PumpData()
        {
            Thread.Sleep(1000);
            Logger.WriteToTrace("Тестирование метода закачки 1");
            Logger.WriteToTrace(RootInDir.ToString());

            //ArchivesTest();
            //FtpTest();
            //ServiceTest();

            if (Convert.ToBoolean(UserParams["deleteData"]))
                Logger.WriteToTrace(UserParams["deleteData"].ToString());
        }

        public void ProcessData()
        {
            Thread.Sleep(3000);
            Logger.WriteToTrace("Тестирование метода закачки 2");
            throw new Exception("Ошибка насяльника!");
        }

        public void CloneData()
        {
            Thread.Sleep(2000);
            Logger.WriteToTrace("Тестирование метода закачки 3", TraceMessageKind.Warning);
            Logger.WriteToTrace(RootOutDir.ToString());
        }
    }
}
