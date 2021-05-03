//Реализация программы без каких-либо средств синхронизации
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
            public AutoResetEvent evFull;
            public AutoResetEvent evEmpty;
            public SemaphoreSlim ssEmpty;
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
            public void ReadSignal(object state)
            {
                var evFull = ((object[])state)[0] as AutoResetEvent;
                var evEmpty = ((object[])state)[1] as AutoResetEvent;
                List<string> MyMessagesRead = new List<string>();//локальный массив читателя
                while (!finish)
                {
                    evFull.WaitOne();//блокируем, ждем сигнала от писателей
                    if (finish)//пока ждали, нам уже сказали прекратить работу
                    {
                        evFull.Set();//даем сигналы следующим читателям
                        break;
                    }
                    MyMessagesRead.Add(buffer);
                    evEmpty.Set();//буфер пуст(прочитали)
                }

                //заносим в статический список, чтобы проверить содержимое
                ResultRea.Add(MyMessagesRead);
            }
            public void WriteSignal(object state)
            {
                var evFull = ((object[])state)[0] as AutoResetEvent;
                var evEmpty = ((object[])state)[1] as AutoResetEvent;
                string[] MyMessagesWri = new string[n];//локальный массив писателя
                for (int j = 0; j < n; j++)
                    MyMessagesWri[j] = j.ToString();
                int i = 0;
                while (i < n)
                {
                    evEmpty.WaitOne();//блокируем, ждем сигнала от читателей
                    buffer = MyMessagesWri[i++];
                    evFull.Set();//буфер заполнен(можно читать)
                }
                //заносим в статический список, чтобы проверить содержимое
                ResultWri.Add(MyMessagesWri);
            }
            public void ReadSemaphore(object o)
            {
                var ssRead = o as SemaphoreSlim;
                List<string> MyMessagesRead = new List<string>();//локальный массив читателя
                while (!finish)
                    if (!bEmpty)
                    {
                        ssRead.Wait();
                        if (!bEmpty)
                        {
                            bEmpty = true;
                            MyMessagesRead.Add(buffer);
                        }
                        ssRead.Release();
                    }
                //заносим в статический список, чтобы проверить содержимое
                ResultRea.Add(MyMessagesRead);
            }
            public void WriteSemaphore(object o)
            {
                var ssWrit = o as SemaphoreSlim;
                string[] MyMessagesWri = new string[n];//локальный массив писателя
                for (int j = 0; j < n; j++)
                    MyMessagesWri[j] = j.ToString();
                int i = 0;
                while (i < n)
                    if (bEmpty)
                    {
                        ssWrit.Wait();
                        if (bEmpty)
                        {
                            buffer = MyMessagesWri[i++];
                            bEmpty = false;
                        }
                        ssWrit.Release();
                    }
                //заносим в статический список, чтобы проверить содержимое
                ResultWri.Add(MyMessagesWri);
            }
        }

        static void ReadWriteFreeAccess(Message varAccess)
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
            ShowMessages(varAccess);
        }
        static void ReadWriteLockAccess(Message varAccess)
        {
            int R = 2, W = 2;
            Thread[] Readers = new Thread[R];
            Thread[] Writers = new Thread[W];
            for (int i = 0; i < W; i++)
            {
                Writers[i] = new Thread(new ThreadStart(varAccess.WriteLock));
                Writers[i].Start();
            }
            for (int i = 0; i < R; i++)
            {
                Readers[i] = new Thread(new ThreadStart(varAccess.ReadLock));
                Readers[i].Start();
            }
            for (int i = 0; i < W; i++)
                Writers[i].Join();
            varAccess.SetFinish(true);//завершаем работу читателей
            for (int i = 0; i < R; i++)
                Readers[i].Join();
            ShowMessages(varAccess);
        }
        static void ReadWriteSignalAccess(Message varAccess)
        {
            int R = 2, W = 2;
            Thread[] Readers = new Thread[R];
            Thread[] Writers = new Thread[W];
            varAccess.evFull = new AutoResetEvent(false);//изначально буфер не полон
            varAccess.evEmpty = new AutoResetEvent(true);//изначально буфер пуст

            for (int i = 0; i < W; i++)
            {
                Writers[i] = new Thread(varAccess.WriteSignal);
                Writers[i].Start(new object[] { varAccess.evFull, varAccess.evEmpty });
            }
            for (int i = 0; i < R; i++)
            {
                Readers[i] = new Thread(varAccess.ReadSignal);
                Readers[i].Start(new object[] { varAccess.evFull, varAccess.evEmpty });
            }
            for (int i = 0; i < W; i++)
                Writers[i].Join();
            varAccess.SetFinish(true);//завершаем работу читателей
            varAccess.evFull.Set();//если читатели не успели прочитать и ждут.
            ShowMessages(varAccess);
        }
        static void ReadWriteSemaphoreAccess(Message varAccess)
        {
            int R = 2, W = 2;
            Thread[] Readers = new Thread[R];
            Thread[] Writers = new Thread[W];

            varAccess.ssEmpty = new SemaphoreSlim(1);//только один запрос может выполняться одновременно
            for (int i = 0; i < W; i++)
            {
                Writers[i] = new Thread(varAccess.WriteSemaphore);
                Writers[i].Start(varAccess.ssEmpty);
            }
            for (int i = 0; i < R; i++)
            {
                Readers[i] = new Thread(varAccess.ReadSemaphore);
                Readers[i].Start(varAccess.ssEmpty);
            }
            for (int i = 0; i < W; i++)
                Writers[i].Join();
            varAccess.SetFinish(true);//завершаем работу читателей
            for (int i = 0; i < R; i++)
                Readers[i].Join();
            ShowMessages(varAccess);
        }
        static void ShowMessages(Message varAccess)
        {
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
            int n = 10000;
            Message freeAccess = new Message(n);
            DateTime dt1, dt2;
            dt1 = DateTime.Now;
            ReadWriteFreeAccess(freeAccess);
            dt2 = DateTime.Now;
            Console.WriteLine((dt2 - dt1).TotalMilliseconds);

            Message lockAccess = new Message(n);
            dt1 = DateTime.Now;
            ReadWriteLockAccess(lockAccess);
            dt2 = DateTime.Now;
            Console.WriteLine((dt2 - dt1).TotalMilliseconds);

            Message signalAccess = new Message(n);
            dt1 = DateTime.Now;
            ReadWriteSignalAccess(signalAccess);
            dt2 = DateTime.Now;
            Console.WriteLine((dt2 - dt1).TotalMilliseconds);

            Message semaphoreAccess = new Message(n);
            dt1 = DateTime.Now;
            ReadWriteSemaphoreAccess(semaphoreAccess);
            dt2 = DateTime.Now;
            Console.WriteLine((dt2 - dt1).TotalMilliseconds);

        }
    }
}
