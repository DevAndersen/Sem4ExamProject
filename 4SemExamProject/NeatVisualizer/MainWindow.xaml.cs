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
        Thread executionThread;

        public MainWindow()
        {
            InitializeComponent();

            controlsToDisable = new List<Control>()
            {
                BtnExecute,
                CbAnimate,
                TbInputs,
                TbOutputs
            };
        }

        private void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            BackgroundGrid.Background = new SolidColorBrush(Color.FromRgb(75, 75, 75));
            executionThread = new Thread(() =>
            {
                ExecuteNeat();
            });
            executionThread.Start();
        }

        private void ExecuteNeat()
        {
            bool wait = false;

            int inputCount = 2;
            int outputCount = 1;

            Dispatcher.Invoke(() =>
            {
                controlsToDisable.ForEach(x => x.IsEnabled = false);
                if (CbAnimate.IsChecked == true)
                {
                    wait = true;
                }

                inputCount = int.TryParse(TbInputs.Text, out inputCount) ? inputCount : 2;
                outputCount = int.TryParse(TbOutputs.Text, out outputCount) ? outputCount : 1;
            });

            Neat neat = new Neat(inputCount, outputCount);

            neat.OnGenerationEnd += (a) =>
            {
                if (wait)
                {
                    DrawAnn(a);
                }
                neat.LowerWaitFlag();
            };

            double[][] inputs = new double[][]
            {
                new double[] { 1, 1 }
            };

            double[][] expectedOutputs = new double[][]
            {
                new double[] { 2, 4 }
            };

            Ann ann = neat.Train(100, 1000, 0.05, 3, 30, wait, inputs, expectedOutputs, Crossover.TwoPointCrossover);

            Dispatcher.Invoke(() =>
            {
                controlsToDisable.ForEach(x => x.IsEnabled = true);
                BackgroundGrid.Background = new SolidColorBrush(Colors.Gray);
            });

            if (!wait)
            {
                DrawAnn(ann);
            }
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            executionThread?.Abort();
        }
    }
}