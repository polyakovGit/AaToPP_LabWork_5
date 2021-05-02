﻿//Реализация программы без каких-либо средств синхронизации
using System;
using System.Collections.Generic;
using System.Threading;
namespace Lab3
{
    class Program
    {

        class Message
        {
            int n;
            bool bEmpty;
            bool finish;
            string buffer;
            //список проверки массивов писателей
            public List<string[]> ResultWri;
            //список проверки массивов читателей
            public List<List<string>> ResultRea;
            public Message(int n)
            {
                this.n = n;
                this.bEmpty = true;
                this.finish = false;
                buffer = null;
                ResultWri = new List<string[]>();
                ResultRea = new List<List<string>>();
            }
            public void SetFinish(bool value)
            {
                finish = value;
            }
            public void Read()
            {
                List<string> MyMessagesRead = new List<string>();//локальный массив читателя
                while (!finish)
                    if (!bEmpty)
                    {
                        MyMessagesRead.Add(buffer);
                        bEmpty = true;
                    }
                ResultRea.Add(MyMessagesRead);
            }
            public void Write()
            {
                string[] MyMessagesWri = new string[n];//локальный массив писателя
                for (int j = 0; j < n; j++)
                    //MyMessagesWri[j] = j.ToString();
                    MyMessagesWri[j] = "Thread WRI #" + Thread.CurrentThread.Name + ", Message: " + j.ToString();
                int i = 0;
                while (i < n)
                    if (bEmpty)
                    {
                        buffer = MyMessagesWri[i++];
                        bEmpty = false;
                    }
                ResultWri.Add(MyMessagesWri);
            }

            public void ReadLock()
            {
                List<string> MyMessagesRead = new List<string>();
                while (!finish)
                    if (!bEmpty)
                    {
                        lock ("read")
                        {
                            if (!bEmpty)
                            {
                                bEmpty = true;
                                MyMessagesRead.Add(buffer);
                            }
                        }
                    }
                ResultRea.Add(MyMessagesRead);
            }
            public void WriteLock()
            {
                string[] MyMessagesWri = new string[n];
                for (int j = 0; j < n; j++)
                    MyMessagesWri[j] = j.ToString();
                int i = 0;
                while (i < n)
                    lock ("write")
                    {
                        if (bEmpty)
                        {
                            buffer = MyMessagesWri[i++];
                            bEmpty = false;
                        }
                    }
                ResultWri.Add(MyMessagesWri);
            }
        }
       static void ReadWriteAccess(Message varAccess)
        {
            int R = 2, W = 2;
            Thread[] Readers = new Thread[R];
            Thread[] Writers = new Thread[W];
            for (int i = 0; i < W; i++)
            {
                Writers[i] = new Thread(new ThreadStart(varAccess.Write));
                Writers[i].Start();
            }
            for (int i = 0; i < R; i++)
            {
                Readers[i] = new Thread(new ThreadStart(varAccess.Read));
                Readers[i].Start();
            }
            for (int i = 0; i < W; i++)
                Writers[i].Join();
            varAccess.SetFinish(true);//завершаем работу читателей
            for (int i = 0; i < R; i++)
                Readers[i].Join();
            int cnt = 0;
            for (int i = 0; i < varAccess.ResultWri.Count; i++)
            {
                cnt += varAccess.ResultWri[i].GetLength(0);
            }
            Console.WriteLine("Всего сообщений отправлено:{0}", cnt);
            cnt = 0;
            for (int i = 0; i < varAccess.ResultRea.Count; i++)
            {
                if (varAccess.ResultRea[i] != null)
                    cnt += varAccess.ResultRea[i].Count;

            }
            Console.WriteLine("Получено сообщений: {0}", cnt);
        }
        static void Main()
        {
            Message freeAccess = new Message(1000);
            DateTime dt1, dt2;
            dt1 = DateTime.Now;
            ReadWriteAccess(freeAccess);
            dt2 = DateTime.Now;
            Console.WriteLine((dt2 - dt1).TotalMilliseconds);

         
        }
    }
}
