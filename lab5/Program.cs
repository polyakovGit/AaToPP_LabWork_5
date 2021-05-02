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
            public Message(int n, bool bEmpty, bool finish)
            {
                this.n = n;
                this.bEmpty = bEmpty;
                this.finish = finish;
                buffer = null;
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
                //заносим в статический список, чтобы проверить содержимое
                //       ResultRea.Add(MyMessagesRead);
            }
            public void Write()
            {
                string[] MyMessagesWri = new string[n];//локальный массив писателя
                for (int j = 0; j < n; j++)
                    MyMessagesWri[j] = j.ToString();
                // MyMessagesWri[j] = "Thread WRI #" + Thread.CurrentThread.Name + ", Message: " + j.ToString();//заменить
                int i = 0;
                while (i < n)
                    if (bEmpty)
                    {
                        buffer = MyMessagesWri[i++];
                        bEmpty = false;
                    }
                //заносим в статический список, чтобы проверить содержимое
                //     ResultWri.Add(MyMessagesWri);
            }
        }

        //список дял проверки массивов писателей
        //static List<string[]> ResultWri = new List<string[]>();
        //список для проверки массивов читателей
        //static List<List<string>> ResultRea = new List<List<string>>();
       
        static void Main()
        {
            Message Serial = new Message(1000000, true, false);
            DateTime dt1, dt2;
            int R = 2; // Параметр N - число писателей
            int W = 2; // Параметр M - число читателей
            Thread[] Writers = new Thread[W];
            Thread[] Readers = new Thread[R];
            dt1 = DateTime.Now;
            for (int i = 0; i < W; i++)
            {
                Writers[i] = new Thread(new ThreadStart(Serial.Write));
                Writers[i].Start();
            }
            for (int i = 0; i < R; i++)
            {
                Readers[i] = new Thread(new ThreadStart(Serial.Read));
                Readers[i].Start();
            }
            for (int i = 0; i < W; i++)
                Writers[i].Join();
            Serial.SetFinish(true);//завершаем работу читателей
            for (int i = 0; i < R; i++)
                Readers[i].Join();
            dt2 = DateTime.Now;
            Console.WriteLine((dt2 - dt1).TotalMilliseconds);
            /*         
                      int cnt = 0;
                      for (int i = 0; i < ResultWri.Count; i++)
                      {
                              cnt += ResultWri[i].GetLength(0);
                      }
                      Console.WriteLine("Всего сообщений отправлено:{0}", cnt);
                      cnt = 0;
                      for (int i = 0; i < ResultRea.Count; i++)
                      {
                          if (ResultRea[i] != null)
                              cnt+= ResultRea[i].Count;

                      }
                      Console.WriteLine("Получено сообщений: {0}",cnt);*/
            Console.ReadKey();
        }
    }
}
