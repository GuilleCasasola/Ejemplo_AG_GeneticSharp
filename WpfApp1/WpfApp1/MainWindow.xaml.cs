using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GeneticSharp;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;

namespace WpfApp1
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bitmap myVar;

        public Bitmap bmpObj
        {
            get { return myVar; }
            set { myVar = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                bmpObj = new Bitmap(@"C:\descarga.bmp", true);
                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmpObj.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(bmpObj.Width, bmpObj.Height));
                ImageBrush ib = new ImageBrush(bs);
                Objetivo.Source = bs;
            }
            catch(Exception e)
            {
                Console.Write(e.ToString());
            }
        }

        private void Iniciar_Click(object sender, RoutedEventArgs e)
        {

          Bitmap bmpDest = new Bitmap(bmpObj.Width,bmpObj.Height);

          Int32[] solucion = new Int32[bmpObj.Width*bmpObj.Height];
            int i=0;
          for(int y = 0; y < bmpObj.Height; y++){
                for (int x = 0; x < bmpObj.Width; x++)
                {
                   System.Drawing.Color color = bmpObj.GetPixel(x, y);
                   solucion[i] = color.ToArgb();
                   i++;
                   //bmpDest.SetPixel(x, y, System.Drawing.Color.FromArgb(acolor));
                }
          }
            
            int tam = bmpObj.Width * bmpObj.Height;
            double[] minArray = new double[tam];
            double[] maxArray = new double[tam];
            int[] bits = new int[tam];
            int[] b2 = new int[tam];
            for (int j = 0; j < tam; j++) {
                minArray[j] = -16777216;
                maxArray[j] = -1;
                bits[j] = 64;
                b2[j] = 0;
            }
            var chromosome = new FloatingPointChromosome(
                    minArray,
                    maxArray,
                    bits,
                    b2
                );

            var fitness = new FuncFitness((c) =>
                {
                    var fc = c as FloatingPointChromosome;
                    double[] values = fc.ToFloatingPoints();
                    //Int32[] valuesAux = new Int32[values.Length];
                    //for (int b = 0; b < values.Length; b++) {
                     //   valuesAux[b] = Convert.ToInt32(values[b]);
                    //}
					double acum;
					acum = 0;
                    for (int j = 0; j < values.Length; j++) {
                        byte[] bA = BitConverter.GetBytes(Convert.ToInt32(values[j]));
                        byte[] bA2 = BitConverter.GetBytes(solucion[j]);
                        int ac = 0;
                        for (int b = 0; b < 4; b++) {
                            ac += Math.Abs(bA[b] - bA2[b]);
                        }
                        acum += ac;
                    }
                    if (acum == 0)
                    {
                        return Int32.MaxValue;
                    }
                    else {
						return 1/  acum;

                    }

                }
            );

            var population = new Population(50, 100, chromosome);

            var selection = new EliteSelection();
            var crossover = new UniformCrossover(0.7f);
            var mutation = new FlipBitMutation();
            var termination = new FitnessStagnationTermination(1000);

            var ga = new GeneticAlgorithm(
                population,
                fitness,
                selection,
                crossover,
                mutation);

            ga.Termination = termination;

          

            var latestFitness = 0.0;
            ga.MutationProbability=0.3f;
            ga.GenerationRan += (a,b) =>
            {
                var bestChromosome = ga.BestChromosome as FloatingPointChromosome;
                var bestFitness = bestChromosome.Fitness.Value;

                if (bestFitness != latestFitness)
                {
                    latestFitness = bestFitness;
                    var phenotype = bestChromosome.ToFloatingPoints();

                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        int pos = 0;
                        for (int y = 0; y < bmpObj.Height; y++)
                        {
                            for (int x = 0; x < bmpObj.Width; x++)
                            {
                               var aas = phenotype[pos];   
                               bmpDest.SetPixel(x, y, System.Drawing.Color.FromArgb(Convert.ToInt32(phenotype[pos])));
                               pos++;
                            }
                        }
                        BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        bmpDest.GetHbitmap(),
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight(bmpDest.Width, bmpDest.Height));
                        ImageBrush ib = new ImageBrush(bs);
                        Destino.Source = bs;


                        //Resta
                        Bitmap resta = new Bitmap(bmpObj.Width, bmpObj.Height);
                        pos = 0;
                        for (int y = 0; y < bmpObj.Height; y++)
                        {
                            for (int x = 0; x < bmpObj.Width; x++)
                            {
                                if (phenotype[pos] - solucion[pos] != 0) {
                                    resta.SetPixel(x, y,
                                    System.Drawing.Color.FromArgb(-16777216)
                                    );
                                }
                                
                                pos++;
                            }
                        }
                        BitmapSource bs2 = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        resta.GetHbitmap(),
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight(bmpDest.Width, bmpDest.Height)); 
                        RestaM.Source = bs2;
                    }));
                   
                }
            };

            Task.Run(()=> {
                ga.Start();
            });
         

        }

       

    }
}
