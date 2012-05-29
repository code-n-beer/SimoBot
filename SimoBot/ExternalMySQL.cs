using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.ComponentModel;

namespace SimoBot
{
    public class ExternalMySQL
    {

        BackgroundWorker bgwQueueExtractor = new BackgroundWorker();

        static object lockDBObject = new object();

        public ExternalMySQL()
        {
            bgwQueueExtractor.DoWork += new DoWorkEventHandler(bgwQueueExtractor_DoWork);
        }

        bool QueueReserved;
        List<string> queue = new List<string>();
        List<string> ReserveQueue = new List<string>();


        private void bgwQueueExtractor_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgwQueueExtractor = sender as BackgroundWorker;
            ExtractQueue();
        }

        public void QueueToExternalMySQL(string[] ParsedMessageArray, string ParsedMsg)
        {

            try
            {
                string MsgReadyForInterwebz = "INSERT INTO msgs VALUES ('" +
                    ParsedMessageArray[0].ToString() + "','" + ParsedMessageArray[1].ToString().Replace(@"Diaz\","Diazzlash").Replace(@"\", "slash") +
                        "','" + ParsedMessageArray[2].ToString() + "','" + ParsedMessageArray[3].ToString() +
                        "','" + ParsedMessageArray[4].ToString().Replace(";", "").Replace("'", "") + "');";

                if (queue.Count >= 10)   // msg frequency of sending lines to external mysql
                {
                    if (!bgwQueueExtractor.IsBusy)
                    bgwQueueExtractor.RunWorkerAsync(); // If there are more than 10 messages in the queue, extract the queue to the internet DB
                }


                if (QueueReserved)
                    AddToReserveQueue(MsgReadyForInterwebz);
                else
                    AddMsgToQueue(MsgReadyForInterwebz);
            }
            catch (Exception eWhole)
            {
                string ExternalMySQLFail = eWhole.Message;
            }
        }

        private void ExtractQueue()
        {
            QueueReserved = true;
            try
            {
                SendQueueToExternalDB(queue);
            }
            catch (Exception SendingFail)
            {
                string WhatFailed = SendingFail.Message;
            }
            queue = ReserveQueue;
            ReserveQueue.Clear();
            QueueReserved = false;
        }

        private void SendQueueToExternalDB(List<string> queue)
        {
            for(int i=0; i < queue.Count ; i++)
            {
                addToDB(queue[i]);
            }
        }

        private void AddMsgToQueue(string MsgReadyForInterwebz)
        {
            queue.Add(MsgReadyForInterwebz);
        }
        private void AddToReserveQueue(string MsgReadyForInterwebz)
        {
            ReserveQueue.Add(MsgReadyForInterwebz);
        }

        public void addToDB(string CommandString)
        {
            string ConnectionString = "SERVER=censored;" + "DATABASE=censored;" + "UID=censored;" + "PWD=censored;"; 
            MySqlConnection conn = new MySqlConnection(ConnectionString);
            MySqlCommand cmd = conn.CreateCommand();
            MySqlDataReader Reader;

            cmd.CommandText = CommandString;
            try
            {
                conn.Open(); 
                Reader = cmd.ExecuteReader();
                //Reader.Read();

                conn.Close();
            }
            catch (MySqlException mysliexepshun)
            {
                string durp = mysliexepshun.Message;
                Console.WriteLine("MySQL failed: " + durp);
            }
        }
    }
}
