//Реализация программы без каких-либо средств синхронизации
using System;
using System.Collections.Generic;
using System.Threading;
namespace Lab3
{
    class Program
    {
        static string buffer;

        //список дял проверки массивов писателей
        //static List<string[]> ResultWri = new List<string[]>();
        //список для проверки массивов читателей
        //static List<List<string>> ResultRea = new List<List<string>>();

        static bool bEmpty = true;
        static bool finish = false;
        static int n = 1000000;
        static void Read()
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
        static void Write( )
        {
            string[] MyMessagesWri = new string[n];//локальный массив писателя
            for (int j = 0; j < (int)n; j++)
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
        static void Main()
        {
            DateTime dt1, dt2;
            int R = 2; // Параметр N - число писателей
            int W = 2; // Параметр M - число читателей
            Thread[] Writers = new Thread[W];
            Thread[] Readers = new Thread[R];
            dt1 = DateTime.Now;
            for (int i = 0; i < W; i++)
            {
                Writers[i] = new Thread(Write);
                Writers[i].Start();
            }
            for (int i = 0; i < R; i++)
            {
                Readers[i] = new Thread(Read);
                Readers[i].Start();
            }
            for (int i = 0; i < W; i++)
                Writers[i].Join();
            finish = true;//завершаем работу читателей
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
