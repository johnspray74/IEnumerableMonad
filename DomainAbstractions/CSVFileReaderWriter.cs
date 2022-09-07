using Nito.AsyncEx;
using ProgrammingParadigms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace DomainAbstractions
{
    /// <summary>
    /// Reads text records from a csv file and outputs them in a class T
    /// Writes text records to a csv file from a class T
    /// CSV file has no header lines
    /// Ports:
    /// This domain abstraction supports three different programming paradigm ports for output, and one port for input.
    ///    Output1: IEnumerable (implemented interface)
    ///    Output2: IObservable (implemented interface)
    ///        With these, reading from the CSV file is initiated by the destination, hence they are both pull programming paradigms despite the fact that IObservable is often taughted as being a
    ///        a push paradigm - it is only push for the data transfer itself, not for starting the transfers, which is done by the Subscribe method.
    ///    Output3: IObserverPush (field) is initiated by the source (this class).
    ///        A start input port is provided for this, however we could add monitoring of the file and push out its content whenever it changes.
    ///        This third output option makes the programming paradigm truly a push programming paradigm.
    /// Input : IObservablePush (implemented interface)  
    ///        From writing to the file - push programming paradigm 
    /// IDataFlow<string>: input for filepath (although filepath can also be set via a property.)
    /// IEvent: input to start file read
    /// 
    /// Example of use:
    ///    private class DataType { public string Name { get; set; } public int Number { get; set; } }
    ///    var csvFileReaderWriter = new CSVFileReaderWriter() { FilePath = "DataFile1.txt" };
    ///    var writer = (IObserverPush<DataType>) csvFileReaderWriter;
    ///    writer.OnStart();
    ///    writer.OnNext(new dataType { Number = 48; Name = "Wynn Takeall"; } ); 
    ///    writer.OnNext(new dataType { Number = 49; Name = "Jim Jones"; } );
    ///    writer.OnCompleted();
    ///    var reader = (IEnumerable<DataType>) csvFileReaderWriter;
    ///    foreach (DataType data in reader) Console.Writeline($"Name={data.Name}, Number={data.Number}");
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
        /// static type version.
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
                T outputObject = ConvertLineToObject<T>(line);
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
                await Task.Delay(1);
                var lines = File.ReadAllLines(filePath);   // TBD want to read a few lines at a time
                await Task.Delay(1);

                foreach (string line in lines)
                {
                    T outputObject = ConvertLineToObject<T>(line);
                    output1?.OnNext(outputObject);
                    output3?.OnNext(outputObject);

                    await Task.Delay(1);
                }
                output1.OnCompleted();
                output1 = null;
                output3?.OnCompleted();
            }
        }



        private T ConvertLineToObject<T>(string line) where T : new()
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
            return outputObject;
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
            if (directoryPath!="" && !Directory.Exists(directoryPath))
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





    /// <summary>
    /// Dynamic version of CSVFileReaderWriter
    /// No static type checking is used.
    /// Reads text records from a csv file and outputs them in a ExpandoObject
    /// Writes text records to a csv file from a ExpandoObject.
    /// CSV file must have two header lines, one for column name, and one for colum type (type.ToString())
    /// Ports:
    /// This domain abstraction supports three different programming paradigm ports for output, and one port for input.
    ///    Output1: IEnumerable (implemented interface)
    ///    Output2: IObservable (implemented interface)
    ///        With these, reading from the CSV file is initiated by the destination, hence they are both pull programming paradigms despite the fact that IObservable is often taughted as being a
    ///        a push paradigm - it is only push for the data transfer itself, not for starting the transfers, which is done by the Subscribe method.
    ///    Output3: IObserverPush (field) is initiated by the source (this class).
    ///        A start input port is provided for this, however we could add monitoring of the file and push out its content whenever it changes.
    ///        This third output option makes the programming paradigm truly a push programming paradigm.
    /// Input : IObservablePush (implemented interface)  
    ///        From writing to the file - push programming paradigm 
    /// IDataFlow<string>: input for filepath (although filepath can also be set via a property.)
    /// IEvent: input to start file read
    /// 
    /// 
    /// Example of use:
    ///    var csvFileReaderWriter = new CSVFileReaderWriter() { FilePath = "DataFile2.txt" };
    ///    IObserverPush<ExpandoObject> writer = csvFileReaderWriter;
    ///    writer.OnStart();
    ///    dynamic eo = new ExpandoObject(); 
    ///    eo.Number = 47; eo.Name = "Jack Up"; writer.OnNext(eo);
    ///    eo.Number = 48; eo.Name = "Wynn Takeall"; writer.OnNext(eo);
    ///    writer.OnCompleted();
    ///    IObservable<ExpandoObject> reader = csvFileReaderWriter;
    ///    reader.Subscribe(
    ///                 (eo) => Console.WriteLine(((IDictionary<string, object>)eo).Select(kvp => kvp.Key + "=" + kvp.Value).Join(", ")),
    ///                 (ex) => Console.WriteLine(ex),
    ///                 () => Console.WriteLine("Completed");
    /// </summary>

    public class CSVFileReaderWriter : IEvent, IDataFlow<string>, IEnumerable<ExpandoObject>, IObservable<ExpandoObject>, IObserverPush<ExpandoObject> // start, filepath, output1, output2, input
    {
        // properties
        public string InstanceName = "Default";
        public string FilePath { set => filePath = value; }

        // ports ----------------------------------------------------------------------------------------
        private IObserverPush<ExpandoObject> output3;

        // private fields ---------------------------------------------------------------------------------
        private int pageSize = 10;
        private IObserver<ExpandoObject> output1;  // This is null except when we have been subscribed to

        /// <summary>
        /// Reading records from csv file and writing records to csv file. 
        /// Dynamic version.
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
        IEnumerator<ExpandoObject> IEnumerable<ExpandoObject>.GetEnumerator()
        {
            if (File.Exists(filePath)) yield break;

            var lines = File.ReadAllLines(filePath);   // TBD want to read a few lines at a time

            string[] columnNames = lines[0].Split(',');
            // get the types of all the columns from the second line
            Type[] types = lines[1].Split(',').Select(columnType => Type.GetType(columnType)).ToArray();

            for (int i = 2; i<lines.Length; i++)
            {
                string[] fields = lines[i].Split(',');
                dynamic outputObject = new ExpandoObject();
                for (int c = 0; c<columnNames.Length; c++)
                {
                    Type t = types[c];
                    ((IDictionary<string, object>)outputObject).Add(columnNames[c], Convert.ChangeType(fields[c], t));
                    c++;
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
        IDisposable IObservable<ExpandoObject>.Subscribe(IObserver<ExpandoObject> observer)
        {
            output1 = observer;
            AsyncContext.Run(() => ReadCSVFilePushAsync());
            return Disposable.Empty;
        }




        // This method used by both the IObservable output port (when it is Subscribed to) and the IEvent start input port
        private async Task ReadCSVFilePushAsync()
        {
            // yield to allow the single thread to do other work (doesn't work to keep UI responsive as UI is lowerest priority)
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
                await Task.Delay(1);
                var lines = File.ReadAllLines(filePath);   // TBD want to read a few lines at a time
                await Task.Delay(1);

                // get the names of the columns from the first line of the CSV file
                string[] columnNames = lines[0].Split(',');
                // get the types of the columns from the second line
                Type[] types = lines[1].Split(',').Select(columnType => Type.GetType(columnType)).ToArray();
                for (int c = 0; c < columnNames.Length; c++)
                {
                    if (columnNames[c] == null || columnNames[c]=="" || !columnNames[c].All(char.IsLetterOrDigit)) 
                        throw new Exception($"Column name in 1st line of {filePath}, \"{lines[0]}\", invalid.");
                    Type t = types[c];
                    if (t==null) throw new Exception($"Column type in 2nd line of {filePath}, \"{lines[1]}\", invalid.");
                    c++;
                }


                for (int i = 2; i < lines.Length; i++)
                {
                    string[] fields = lines[i].Split(',');
                    dynamic outputObject = new ExpandoObject();
                    for (int c = 0; c < columnNames.Length; c++)
                    {
                        ((IDictionary<string, object>)outputObject).Add(columnNames[c], Convert.ChangeType(fields[c], types[c]));
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
        // IEnumerable is not suitable because we usually want to initiate a write from the source, not the destination
        // standard Iobservable is not suitable, again we usually want to initiate the transfer from the source.
        private bool started = false;
        private StringBuilder stringBuilder = null;
        private int index;

        void IObserverPush<ExpandoObject>.OnStart()
        {
            index = 0;
            stringBuilder = new StringBuilder();
            started = true;
        }

        void IObserverPush<ExpandoObject>.OnNext(ExpandoObject eo)
        {
            if (started)
            {
                IDictionary<string, object> dic = eo;

                bool first;
                if (index==0)
                {
                    first = true;
                    foreach (KeyValuePair<string, object> kvp in dic.AsEnumerable())
                    {
                        if (!first) stringBuilder.Append(',');
                        stringBuilder.Append(kvp.Key);
                        first = false;
                    }
                    stringBuilder.Append(Environment.NewLine);
                    first = true;
                    foreach (KeyValuePair<string, object> kvp in dic.AsEnumerable())
                    {
                        if (!first) stringBuilder.Append(',');
                        stringBuilder.Append(kvp.Value.GetType());
                        first = false;
                    }
                    stringBuilder.Append(Environment.NewLine);
                }
                first = true;
                foreach (KeyValuePair<string, object> kvp in dic.AsEnumerable())
                {
                    if (!first) stringBuilder.Append(',');
                    stringBuilder.Append(kvp.Value);
                    first = false;
                }
                index++;
                stringBuilder.Append(Environment.NewLine);
            }
        }


        void IObserverPush<ExpandoObject>.OnCompleted()
        {
            if (started)
            {
                string path = Path.GetDirectoryName(filePath);
                if (path!=null && path!="" && !Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // all the data loaded, write them to the file and output the finish signal
                File.WriteAllText(filePath, stringBuilder.ToString());
                stringBuilder.Clear();
                started = false;
            }
        }

        void IObserverPush<ExpandoObject>.OnError(Exception ex)
        {
            if (started)
            {
                stringBuilder.Append(ex);
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
}
