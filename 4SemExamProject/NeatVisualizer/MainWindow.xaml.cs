using DatabaseNormalizer;
using DatabaseNormalizer.DatabaseHandlers;
using NeatLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace NeatVisualizer
{
    public partial class MainWindow : Window
    {
        List<Control> controlsToDisable;
        List<Control> controlsToEnable;
        Thread executionThread;
        bool pause = false;

        public MainWindow()
        {
            InitializeComponent();

            controlsToDisable = new List<Control>()
            {
                BtnExecute,
                CbAnimate,
                TbInputs,
                TbOutputs,
                TbDelay
            };

            controlsToEnable = new List<Control>()
            {
                BtnPause
            };
        }

        private void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            SVCanvas.Background = new SolidColorBrush(Color.FromRgb(75, 75, 75));
            executionThread = new Thread(() =>
            {
                ExecuteNeat();
            });
            executionThread.Start();
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if(executionThread != null && executionThread.IsAlive)
            {
                pause = !pause;
                BtnPause.Content = pause ? "Unpause" : "Pause";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                executionThread?.Abort();
            }
            catch { }
        }

        private void ExecuteNeat()
        {
            bool wait = false;

            int inputCount = 2;
            int outputCount = 1;
            int delay = 150;

            Dispatcher.Invoke(() =>
            {
                controlsToDisable.ForEach(x => x.IsEnabled = false);
                controlsToEnable.ForEach(x => x.IsEnabled = true);
                if (CbAnimate.IsChecked == true)
                {
                    wait = true;
                }

                inputCount = int.TryParse(TbInputs.Text, out inputCount) ? inputCount : 2;
                outputCount = int.TryParse(TbOutputs.Text, out outputCount) ? outputCount : 1;
                delay = int.TryParse(TbDelay.Text, out delay) ? delay : 0;
            });

            Neat neat = new Neat();

            neat.OnGenerationEnd += (a) =>
            {
                Dispatcher.Invoke(() =>
                {
                    TBSynapseData.Clear();
                    a.synapses.ForEach(x =>
                    {
                        TBSynapseData.Text += "FromLayer: " + x.FromLayer + "\n";
                        TBSynapseData.Text += "Fromneuron: " + x.FromNeuron + "\n";
                        TBSynapseData.Text += "ToLayer: " + x.ToLayer + "\n";
                        TBSynapseData.Text += "ToNeuron: " + x.ToNeuron + "\n";
                        TBSynapseData.Text += "Weight: " + x.Weight + "\n";
                        TBSynapseData.Text += "\n";
                    });

                    TBNeuronData.Clear();
                    a.hiddenNeurons.ForEach(x =>
                    {
                        TBNeuronData.Text += "Layer: " + x.Layer + "\n";
                        TBNeuronData.Text += "NeuronPosition: " + x.NeuronPosition + "\n";
                        TBNeuronData.Text += "Bias: " + x.Bias + "\n";
                        TBNeuronData.Text += "\n";
                    });

                    TBOutputNeurons.Clear();
                    a.outputNeurons.ToList().ForEach(x =>
                    {
                        TBOutputNeurons.Text += "Value: " + x.Value + "\n";
                        TBOutputNeurons.Text += "\n";
                    });
                });
                if (wait)
                {
                    DrawAnn(a);
                    Thread.Sleep(delay);
                }
                while(pause)
                {

                }
                neat.LowerWaitFlag();
            };


            string[] inputColumns = new string[]
            {
                //"movie_title",
                "genres",
                "director_name",
                "budget",
            };

            string[] expectedOutputColumns = new string[]
            {
                "gross",
            };

            NormalizedDataAndDictionaries inputData = GetDataFromDatabase(inputColumns);
            NormalizedDataAndDictionaries outputData = GetDataFromDatabase(expectedOutputColumns);

            //Ann ann = neat.Train(10000, 100, 0.1, 3, 30, wait, inputs, expectedOutputs, Crossover.TwoPointCrossover, ActivationFunction.Identity);
            //Ann ann = neat.Train(1000, 100, 0.03, 3, 30, false, inputs, expectedOutputs, Crossover.TwoPointCrossover, ActivationFunction.Identity);
            //Ann ann = neat.Train(1000, 100, 0.03, 3, 30, false, inputData.NormalizedData, outputData.NormalizedData, Crossover.BestParentClone, ActivationFunction.Sigmoid);
            Ann ann = neat.Train(1000, 100, 0.03, 3, 30, false, inputData.NormalizedData, outputData.NormalizedData, Crossover.BestParentClone, ActivationFunction.Identity);

            Dispatcher.Invoke(() =>
            {
                controlsToDisable.ForEach(x => x.IsEnabled = true);
                controlsToEnable.ForEach(x => x.IsEnabled = false);
                SVCanvas.Background = new SolidColorBrush(Colors.Gray);
            });

            if (!wait)
            {
                DrawAnn(ann);
            }
        }

        private NormalizedDataAndDictionaries GetDataFromDatabase(string[] columns)
        {
            string condition = "WHERE actor_1_name = 'Robert Downey Jr.'";
            string databaseLocation = AppDomain.CurrentDomain.BaseDirectory;
            databaseLocation = databaseLocation.Replace(@"NeatVisualizer\bin\Debug\", @"DatabaseNormalizer\database\4SemExamProject.mdf");
            return DataManager.GetNormalizedDataFromDatabase(new DatabaseHandlerSQL(), databaseLocation, columns, condition, 0, 1, 0.25);
        }

        #region Draw

        private void DrawAnn(Ann ann)
        {
            Dispatcher.Invoke(() =>
            {
                Canvas.Children.Clear();
            });

            int canvasMaxX = 0;
            int canvasMaxY = 0;

            int neuronSize = 20;
            int tilesize = 50;

            int synapseOffset = neuronSize / 2;

            ann.synapses.ForEach(x =>
            {
                if (x.FromLayer >= 0 && x.ToLayer >= 0)
                {
                    DrawLine(x.FromLayer * tilesize + synapseOffset + tilesize, x.FromNeuron * tilesize + synapseOffset, x.ToLayer * tilesize + synapseOffset + tilesize, x.ToNeuron * tilesize + synapseOffset, x.Weight);

                    if (canvasMaxX < x.FromLayer * tilesize) canvasMaxX = x.FromLayer * tilesize;
                    if (canvasMaxY < x.FromNeuron * tilesize) canvasMaxY = x.FromNeuron * tilesize;
                    if (canvasMaxX < x.ToLayer * tilesize) canvasMaxX = x.ToLayer * tilesize;
                    if (canvasMaxY < x.ToNeuron * tilesize) canvasMaxY = x.ToNeuron * tilesize;
                }
            });

            ann.GetAllLayers().ToList().ForEach(x =>
            {
                if (x >= 0)
                {
                    ann.GetNeuronsForLayer(x).ToList().ForEach(y =>
                    {
                        DrawCircle(x * tilesize + tilesize, y.NeuronPosition * tilesize, neuronSize, NeuronType.HIDDEN);

                        if (canvasMaxX < x * tilesize) canvasMaxX = x * tilesize;
                        if (canvasMaxY < y.NeuronPosition * tilesize) canvasMaxY = y.NeuronPosition * tilesize;
                    });
                }
            });

            ann.synapses.ForEach(x =>
            {
                if (x.FromLayer < 0 || x.ToLayer < 0)
                {
                    int xFrom = x.FromLayer * tilesize + tilesize;
                    int xTo = x.ToLayer * tilesize + tilesize;

                    if (x.FromLayer == (int)Neuron.BaseNeuronType.Input)
                        xFrom = 0;

                    if (x.ToLayer == (int)Neuron.BaseNeuronType.Input)
                        xTo = 0;

                    if (x.FromLayer == (int)Neuron.BaseNeuronType.Output)
                        xFrom = canvasMaxX + tilesize * 2;

                    if (x.ToLayer == (int)Neuron.BaseNeuronType.Output)
                        xTo = canvasMaxX + tilesize * 2;

                    DrawLine(synapseOffset + xFrom, x.FromNeuron * tilesize + synapseOffset, synapseOffset + xTo, x.ToNeuron * tilesize + synapseOffset, x.Weight);
                    if (canvasMaxY < x.FromNeuron * tilesize) canvasMaxY = x.FromNeuron * tilesize;
                    if (canvasMaxY < x.ToNeuron * tilesize) canvasMaxY = x.ToNeuron * tilesize;
                }
            });

            ann.GetAllLayers().ToList().ForEach(x =>
            {
                if (x == (int)Neuron.BaseNeuronType.Input)
                {
                    ann.GetNeuronsForLayer(x).ToList().ForEach(y =>
                    {
                        DrawCircle(0, y.NeuronPosition * tilesize, neuronSize, NeuronType.INPUT);
                        if (canvasMaxY < y.NeuronPosition * tilesize) canvasMaxY = y.NeuronPosition * tilesize;
                    });
                }
            });

            ann.GetAllLayers().ToList().ForEach(x =>
            {
                if (x == (int)Neuron.BaseNeuronType.Output)
                {
                    ann.GetNeuronsForLayer(x).ToList().ForEach(y =>
                    {
                        DrawCircle(canvasMaxX + tilesize * 2, y.NeuronPosition * tilesize, neuronSize, NeuronType.OUTPUT);
                        if (canvasMaxY < y.NeuronPosition * tilesize) canvasMaxY = y.NeuronPosition * tilesize;
                    });
                }
            });

            Dispatcher.Invoke(() =>
            {
                LblNeurons.Content = "Neurons: " + ann.hiddenNeurons.Count;
                LblSynapses.Content = "Synapses: " + ann.synapses.Count;
                LblGeneration.Content = "Generation " + ann.Generation;
                LblError.Content = "Error: " + ann.Error;

                Canvas.Width = canvasMaxX + tilesize * 2.5;
                Canvas.Height = canvasMaxY + tilesize * 0.5;
            });
        }

        private void DrawLine(int x1, int y1, int x2, int y2, double thickness)
        {
            Dispatcher.Invoke(() =>
            {
                Line line = new Line()
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = thickness * 2
                };
                Canvas.Children.Add(line);
            });
        }

        private void DrawCircle(int x, int y, int size, NeuronType neuronType)
        {

            Dispatcher.Invoke(() =>
            {
                Ellipse ellipse = new Ellipse()
                {
                    Height = size,
                    Width = size,
                    Stroke = new SolidColorBrush(Colors.Black),
                    Fill = new SolidColorBrush(neuronType == NeuronType.HIDDEN ? Colors.Wheat : neuronType == NeuronType.INPUT ? Colors.LightGreen : Colors.LightPink)
                };
                Canvas.SetLeft(ellipse, x);
                Canvas.SetTop(ellipse, y);
                Canvas.Children.Add(ellipse);
            });
        }

        private enum NeuronType
        {
            HIDDEN,
            INPUT,
            OUTPUT
        }

        #endregion
    }
}