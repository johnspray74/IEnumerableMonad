using Nito.AsyncEx;
using ProgrammingParadigms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace DomainAbstractions
{
    /// <summary>
    /// Reading text records from a csv file and outputs them in a class T.
    /// Writing text records to a csv file from a class T.
    /// Ports
    /// This domain abstraction supports three different programming paradigms options for output, and one option for input.
    ///    Output1: IEnumerable (implemented)
    ///    Output2: IObservable (implemented)
    ///    With these, data transfers are initiated by the destination, hence they are both pull programming paradigms destpite the fact that IObservable is often taughted as being a
    ///    a push paradigm - it is only push for the data transfer itself, not for starting the transfers, which is done by the Subscribe method.
    ///    Output3: IObserverPush (field) is initiated by the source which is this class.
    ///    A start input is provided for this, however we could add monitoring of the file and push out its contenst if it changes.
    ///    This third output option makes the programming paradigm truly a push programming paradigm.
    /// Input : IObservablePush (implemented)   
    /// IDataFlow<string>: input for filepath;
    /// iEvent: input to start file read
    /// </summary>
    public class CSVFileReaderWriter<T> : IEvent, IDataFlow<string>, IEnumerable<T>, IObservable<T>, IObserverPush<T> where T : new() // start, filepath, output1, output2, input
    {
        // properties
        public string InstanceName = "Default";
        public string FilePath { set => filePath = value; }

        // ports ----------------------------------------------------------------------------------------
        private IObserverPush<T> output3;

        // private fields ---------------------------------------------------------------------------------
        private int pageSize = 10;
        private IObserver<T> output1;  // This is null except when we have been subscribed to

        /// <summary>
        /// Reading records from csv file and writing records to csv file. 
        /// The only accepted data type is ITableDataFlow.
        /// </summary>
        public CSVFileReaderWriter() { }




        // IDataFlow<string> implementation ------------------------------------------------------
        private string filePath;
        void IDataFlow<string>.Push(string value) { filePath = value; }




        void IEvent.Execute()
        {
            // Task.Run(() => ReadCSVFileAsync()); // this runs it on another thread. I want a single threaded application.
            AsyncContext.Run(() => ReadCSVFilePushAsync());
        }






        // --------------------------------------------------------------------------------------------------
        // implement IEnumerable output port
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            var lines = File.Exists(filePath) ?
                File.ReadAllLines(filePath) : new string[] { "" };   // TBD want to read a few lines at a time

            foreach (string line in lines)
            {
                string[] fields = line.Split(',');
                var outputObject = new T();
                int i = 0;
                foreach (var property in typeof(T).GetProperties())
                {
                    if (i == fields.Length) break;
                    property.SetValue(outputObject, Convert.ChangeType(fields[i], typeof(T)));
                    i++;
                }
                yield return outputObject;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }







        // --------------------------------------------------------------------------------------------------
        // implement IObservable output port
        IDisposable IObservable<T>.Subscribe(IObserver<T> observer)
        {
            output1 = observer;
            AsyncContext.Run(() => ReadCSVFilePushAsync());
            return Disposable.Empty;
        }




        // This method used by both the IObservable output port (when it is Subscribed to) and the IEvent start input port
        private async Task ReadCSVFilePushAsync()
        {
            // yield to allow the single thread to do other work 
            // await Task.Yield(); // UI tasks are lower priority so this keep the UI responsive
            await Task.Delay(1); // This 1ms will allow at least one UI or paint task to run  

            if (!File.Exists(filePath))
            {
                var ex = new Exception("File not found");
                output1?.OnError(ex); ;
                output1 = null;
                output3?.OnError(ex);
            }
            else
            {
                var lines = File.ReadAllLines(filePath);   // TBD want to read a few lines at a time
                await Task.Delay(1);

                foreach (string line in lines)
                {
                    string[] fields = line.Split(',');
                    var outputObject = new T();
                    int i = 0;
                    foreach (var property in typeof(T).GetProperties())
                    {
                        if (i == fields.Length) break;
                        property.SetValue(outputObject, Convert.ChangeType(fields[i], property.PropertyType));
                        i++;
                    }
                    output1?.OnNext(outputObject);
                    output3?.OnNext(outputObject);

                    await Task.Delay(1);
                }
                output1.OnCompleted();
                output1 = null;
                output3?.OnCompleted();
            }
        }




        // --------------------------------------------------------------------------------------------------
        // implement IObserverPush input port
        // This is the only interface that writes to the CSV file
        // IEnumerable is not suitable because we usually want to initiate a write from the source
        // standard Iobservable is not suitable, again we usually want to initiate the transfer from the source.
        private bool started = false;
        private StringBuilder stringBuilder = null;
        private int index;

        void IObserverPush<T>.OnStart()
        {
            index = 0;
            stringBuilder = new StringBuilder();
            started = true;
        }

        void IObserverPush<T>.OnNext(T record)
        {
            foreach (var property in typeof(T).GetProperties())
            {
                stringBuilder.Append(property.GetValue(record).ToString() + ",");
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append(Environment.NewLine);
        }


        void IObserverPush<T>.OnCompleted()
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // all the data loaded, write them to the file and output the finish signal
            File.WriteAllText(filePath, stringBuilder.ToString());
            stringBuilder.Clear();
            started = false;
        }

        void IObserverPush<T>.OnError(Exception ex)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(filePath, stringBuilder.ToString());
            stringBuilder.Clear();
        }

    }
}
