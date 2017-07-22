using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace tdc2017sp_techwar_thread_v2
{
    class Program
    {
        static void Main(string[] args)
        {
            //Laser Implementatios Options
            //BasicLaser
            //LaserWithThread
            //LaserWithThreadPool
            //LaserWithTasks

            var deathStar = new DeathStar(new BasicLaser());
            var maximumSeconds = 4;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("---------------------------------------------");
            Console.WriteLine($"A long time ago in a galaxy far, far away....");
            Console.WriteLine("---------------------------------------------");
            Console.ReadKey();
            Console.WriteLine($"TechWars");
            Console.WriteLine($"Alternative Episode TDC SP 2017");
            Console.WriteLine($"It is a period of civil war....");
            Console.WriteLine($"DeathStar is under attack by Rebels Force.");
            Console.WriteLine($"If DeathStar NOT charge SuperLaser before {maximumSeconds} seconds, Luke Skywalker will destroy it.");
            Console.WriteLine("---------------------------------------------");
            Console.ResetColor();
            Console.ReadKey();

            Console.WriteLine($"Darth Vader ordered to activate DeathStar`s SuperLaser.");
            deathStar.SuperLaser.ReloadReactors();

            var secondsElapsed = deathStar.SuperLaser.SecondsElapsed;

            if (secondsElapsed <= maximumSeconds)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                deathStar.SuperLaser.Shot();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                deathStar.Destroy();
            }

            Console.WriteLine($"Seconds Elapsed: {secondsElapsed}");
            Console.ReadLine();
        }
    }

    public class DeathStar
    {
        public DeathStar(ILaser laser)
        {
            SuperLaser = laser;
        }

        public ILaser SuperLaser { get; private set; }

        public void Destroy()
        {
            Console.WriteLine("DeathStar was destroyed by Luke and Rebels Force.");
        }
    }

    public interface ILaser
    {
        bool IsReady { get; }
        double SecondsElapsed { get; }

        void ReloadReactors();
        void Shot();
    }

    public class BasicLaser : ILaser
    {
        protected int batteryPercentage = 0;
        protected Stopwatch watch = new Stopwatch();

        public bool IsReady { get { return batteryPercentage >= 100; } }

        public double SecondsElapsed { get { return watch.Elapsed.TotalSeconds; } }
        public virtual void ReloadReactors()
        {
            watch.Start();
            
            for (var number = 1; number <= 11; number++)
            {
                ReloadOneReactor(number);
            }

            watch.Stop();
        }

        protected void ReloadOneReactor(int number)
        {
            if (!IsReady)
            {
                batteryPercentage += 10;
                Console.WriteLine($"{number}-Reload Reactors Percentage: {batteryPercentage} %");
                Thread.Sleep(1000);
            }
        }
        public void Shot()
        {
            if (!IsReady)
            {
                Console.WriteLine($"SuperLaser is not ready. Oh No !");
                return;
            }

            Console.WriteLine("DeathStar Super Laser Released. One more planet destroyed. Yeahh !!");
            batteryPercentage = 0;
        }
    }

    public class LaserWithThread : BasicLaser
    {
        public override void ReloadReactors()
        {
            var myThreads = new List<Thread>();
            watch.Start();

            for (var number = 1; number <= 11; number++)
            {
                var thread = new Thread(() =>
                {
                    ReloadOneReactor(number);
                });

                thread.Name = $"Thread-{batteryPercentage}";
                myThreads.Add(thread);
                thread.Start();
            }

            bool allThreadsFinished = false;
            while (!allThreadsFinished)
            {
                allThreadsFinished = true;
                foreach (var thread in myThreads)
                {
                    if (thread.IsAlive)
                    {
                        allThreadsFinished = false;
                    }
                }

                Thread.Sleep(100);
            }

            watch.Stop();

        }
    }

    public class LaserWithThreadPool : BasicLaser
    {
        public override void ReloadReactors()
        {
            var handles = new List<ManualResetEvent>();
            watch.Start();
            
            for (var number = 1; number <= 11; number++)
            {
                var handle = new ManualResetEvent(false);
                handles.Add(handle);

                ThreadPool.SetMaxThreads(1000, 4);

                ThreadPool.QueueUserWorkItem(new WaitCallback(arg =>
                {
                    ReloadOneReactor(number);
                    handle.Set();
                }));
            }

            WaitHandle.WaitAll(handles.ToArray());

            watch.Stop();
        }
    }

    public class LaserWithTasks : BasicLaser
    {
        public override void ReloadReactors()
        {
            Task.Run(() =>
            {
                watch.Start();
                Parallel.For(1, 11, number => { ReloadOneReactor(number); });
                watch.Stop();
            });
        }
    }
}
