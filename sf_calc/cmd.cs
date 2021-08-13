using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Numerics;

namespace Cmd2
{
    class Program
    {
        public void main()
        {

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            Char[] SepString = new Char[] { ' ', '\t', '=', '#', ',' };
            
           
            BigDecimal[] ns = new BigDecimal[402];
            BigDecimal[] ds = new BigDecimal[402];

            sw.Start();

            for (int i = 0; i < 401; i++)
            {
                ns[i] = BigDecimal.Zero;
                ds[i] = BigDecimal.Zero;
            }
            

            StreamReader infs = new StreamReader("RC.txt");

            String line = infs.ReadLine();
            string[] TextArr1;
            int n = 0;

            while ((line = infs.ReadLine()) != null)
            {
                try
                {
                    TextArr1 = line.Split(SepString, StringSplitOptions.RemoveEmptyEntries);
                    decimal r = (decimal)System.Convert.ToDouble(TextArr1[0]);
                    BigDecimal RR = new BigDecimal(r);
                    decimal c = (decimal)System.Convert.ToDouble(TextArr1[1]);
                    BigDecimal CC = new BigDecimal(c);
                    BigDecimal RC = RR * CC;

                    if (n == 0)
                    {
                        ns[0] = RR;
                        ds[0] = BigDecimal.One;
                        ds[1] = RC;
                    }

                    if (r > 0 && c > 0)
                    {
                        n++;
                        ds[n + 1] = ds[n] * RC;
                        for (int i = n; i > 0; i--)
                        {
                            ns[i] = ns[i] + ns[i - 1] * RC + ds[i] * RR;
                            ds[i] = ds[i] + ds[i - 1] * RC;
                        }
                        ns[0] = ns[0] + ds[0] * RR; ; 
                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                //Console.WriteLine(n);
            }
            n++;
            int Nmax = n;
            infs.Close();

            
            BigDecimal[] rn = new BigDecimal[402];
            BigDecimal[] cn = new BigDecimal[402];

            for (int i = 0; i < Nmax; i++) {
                cn[i] = ds[Nmax - i] / ns[Nmax - i - 1];
                ds[Nmax-i]=0;
                for (int j = 1; j < n; j++) {
                    ds[j] -= ns[j - 1] * cn[i];
                }
                rn[i] = ns[Nmax - i - 1] / ds[Nmax - i - 1];
                
                ns[Nmax - i - 1]=0;
                for (int j = 0; j < n; j++) {
                    ns[j] -= ds[j] * rn[i];
                }
                if (rn[i] < BigDecimal.Zero) rn[i] = BigDecimal.Zero;
                if (cn[i] < BigDecimal.Zero) cn[i] = BigDecimal.Zero;
                n--;
                
            }

            BigDecimal r1 = BigDecimal.Zero;
            BigDecimal c1 = BigDecimal.Zero;
            StreamWriter outf = new StreamWriter("RnCn.CSV");
            for (int i = 0; i < Nmax; i++) {
                c1 += cn[i];
                outf.WriteLine("{0},{1}", (double)r1, (double)c1);
                r1 += rn[i];
            }
            outf.Close();
            sw.Stop();
            Console.WriteLine("{0} : 経過時間 {1}", n, sw.Elapsed);

        }
    }
}
