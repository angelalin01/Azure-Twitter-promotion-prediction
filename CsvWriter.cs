using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

//Writes all attribute data of each batch of random tweet sampe in a CSV file 
public class CsvWriter
{
    private const string DELIMITER = ",";// based on comma format of CSV file

    public string Write<T>(IList<T> list, bool includeHeader = true)
    {
        StringBuilder sb = new StringBuilder();
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties();

        if (includeHeader)
        {
            sb.AppendLine(this.CreateCsvHeaderLine(properties));
        }
        foreach (var item in list)
        {
            sb.AppendLine(this.CreateCsvLine(item, properties));
        }

        return sb.ToString();
    }

    public string Write<T>(IList<T> list, string fileName, bool includeHeader = true)
    {
        string csv = this.Write(list, includeHeader);
        this.WriteFile(fileName, csv);
        return csv;
    }

    private string CreateCsvHeaderLine(PropertyInfo[] properties)//based on attribute features
    {
        List<string> propertyValues = new List<string>();

        foreach (var prop in properties)
        {
            string stringformatString = string.Empty;
            string value = prop.Name;

            var attribute = prop.GetCustomAttribute(typeof(DisplayAttribute));
            if (attribute != null)
            {
                value = (attribute as DisplayAttribute).Name;
            }

            this.CreateCsvStringItem(propertyValues, value);
        }

        return this.CreateCsvLine(propertyValues);
    }

    private string CreateCsvLine<T>(T item, PropertyInfo[] properties)
    {
        List<string> propertyValues = new List<string>();

        foreach (var prop in properties)
        {
            string stringformatString = string.Empty;
            object value = prop.GetValue(item, null);

            if (prop.PropertyType == typeof(string))
            {
                this.CreateCsvStringItem(propertyValues, value);
                //} else if (prop.PropertyType == typeof(string[])) {
                //    this.CreateCsvStringArrayItem(propertyValues, value);
            }
            else if (prop.PropertyType == typeof(List<string>))
            {
                this.CreateCsvStringListItem(propertyValues, value);
            }
            else
            {
                this.CreateCsvItem(propertyValues, value);
            }
        }

        return this.CreateCsvLine(propertyValues);
    }

    private string CreateCsvLine(IList<string> list)
    {
        return string.Join(CsvWriter.DELIMITER, list);
    }

    private void CreateCsvItem(List<string> propertyValues, object value)
    {
        if (value != null)
        {
            propertyValues.Add(value.ToString());
        }
        else
        {
            propertyValues.Add(string.Empty);
        }
    }

    private void CreateCsvStringListItem(List<string> propertyValues, object value)
    {
        string formatString = "\"{0}\"";
        if (value != null)
        {
            value = this.CreateCsvLine((List<string>)value);
            propertyValues.Add(string.Format(formatString, this.ProcessStringEscapeSequence(value)));
        }
        else
        {
            propertyValues.Add(string.Empty);
        }
    }

    //private void CreateCsvStringArrayItem(List<string> propertyValues, object value) {
    //    string formatString = "\"{0}\"";
    //    if (value != null) {
    //        value = this.CreateCsvLine(((string[]) value).ToList());
    //        propertyValues.Add(string.Format(formatString, this.ProcessStringEscapeSequence(value)));
    //    } else {
    //        propertyValues.Add(string.Empty);
    //    }
    //}

    private void CreateCsvStringItem(List<string> propertyValues, object value)
    {
        string formatString = "\"{0}\"";
        if (value != null)
        {
            propertyValues.Add(string.Format(formatString, this.ProcessStringEscapeSequence(value)));
        }
        else
        {
            propertyValues.Add(string.Empty);
        }
    }

    private string ProcessStringEscapeSequence(object value)
    {
        return value.ToString().Replace("\"", "\"\"");
    }

    public bool WriteFile(string fileName, string csv)
    {
        bool fileCreated = false;

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            File.WriteAllText(fileName, csv);

            fileCreated = true;
        }

        return fileCreated;
    }
}