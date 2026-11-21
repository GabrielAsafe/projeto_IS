namespace WaterValve;

using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

public class XmlNotificationStorage
{
    private readonly string _xmlPath;
    private readonly string _xsdPath;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public XmlNotificationStorage(string xmlPath, string xsdPath)
    {
        _xmlPath = xmlPath;
        _xsdPath = xsdPath;

        if (!File.Exists(_xmlPath))
        {
            var doc = new XDocument(new XElement("notifications"));
            doc.Save(_xmlPath);
        }
    }

    public async Task SaveAsync(NotificationPayload payload)
    {
        await _lock.WaitAsync();
        try
        {
            var doc = XDocument.Load(_xmlPath);

            var element = new XElement("notification",
                new XElement("event", payload.Event),
                new XElement("path", payload.Path),
                new XElement("resource",
                    new XElement("res-type",      payload.Resource!.ResType),
                    new XElement("resource-name", payload.Resource.ResourceName),
                    new XElement("content-type",  payload.Resource.ContentType),
                    new XElement("content",       payload.Resource.Content),
                    new XElement("dt-creation",   payload.Resource.DtCreation),
                    new XElement("dt-expiration", payload.Resource.DtExpiration)
                ),
                new XElement("receivedAt",
                    DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"))
            );

            var errors = GetValidationErrors(doc, element);
            if (errors.Count > 0)
            {
                element.Add(new XElement("validationErrors",
                    errors.Select(e => new XElement("error", e))
                ));
                Console.WriteLine($"XML validation failed: {string.Join(", ", errors)}");
            }
            else
            {
                Console.WriteLine("Notification saved to XML.");
            }


            doc.Root!.Add(element);
            doc.Save(_xmlPath);

        }

        finally
        {
            _lock.Release();
        }
    }

    private List<string> GetValidationErrors(XDocument existingDoc, XElement newElement)
    {
        var errors = new List<string>();

        if (!File.Exists(_xsdPath))
            return errors;

        var testDoc = new XDocument(existingDoc);
        testDoc.Root!.Add(newElement);

        var schemas = new XmlSchemaSet();
        schemas.Add("", _xsdPath);

        testDoc.Validate(schemas, (o, e) =>
        {
            errors.Add(e.Message);
        });

        return errors;
    }
}