using System;
using System.Collections.Generic;
using Final.Entity; // For TopicDataType and TopicPurpose

namespace Final.Models.LiveData
{
    #nullable disable
    /// <summary>
    /// A generic model for tool live data.
    /// Mirrors key fields from the MqttTool entity and contains live topic data.
    /// </summary>
    public class GenericToolModel
    {
        /// <summary>
        /// The internal numeric ID used for live-data indexing.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The original identifier from the MqttTool entity.
        /// </summary>
        public Guid ToolId { get; set; }

        /// <summary>
        /// The name of the tool.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The base topic associated with the tool.
        /// </summary>
        public string ToolBaseTopic { get; set; }

        /// <summary>
        /// A description for the tool.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// An image URL for display purposes.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// The company identifier to which the tool belongs.
        /// </summary>
        public Guid CompanyId { get; set; }

        /// <summary>
        /// A collection of topics (with live data) associated with the tool.
        /// Each topic corresponds to a configured MqttTopic.
        /// </summary>
        public List<GenericTopicModel> Topics { get; set; } = new List<GenericTopicModel>();

        /// <summary>
        /// A flexible dictionary to store additional live values or computed data.
        /// For instance, you might add aggregated metrics or status flags.
        /// </summary>
        public Dictionary<string, object> LiveValues { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// A helper model to encapsulate live data for a given MQTT topic.
    /// This model mirrors fields from your MqttTopic entity.
    /// </summary>
    public class GenericTopicModel
    {
        /// <summary>
        /// The original identifier from the MqttTopic entity.
        /// </summary>
        public Guid TopicId { get; set; }

        /// <summary>
        /// The base topic (e.g. a company-specific prefix).
        /// </summary>
        public string BaseTopic { get; set; }

        /// <summary>
        /// The full topic template, which may include parameters.
        /// </summary>
        public string TopicTemplate { get; set; }

        /// <summary>
        /// A value indicating how many topic instances or parameters are expected.
        /// </summary>
        public int HowMany { get; set; }

        /// <summary>
        /// The data type of the topic value.
        /// </summary>
        public TopicDataType DataType { get; set; }

        /// <summary>
        /// Indicates if this topic is used for subscriptions or sending data.
        /// </summary>
        public TopicPurpose TopicPurpose { get; set; }

        /// <summary>
        /// A fixed-length byte array for raw data, similar to the Data64 field.
        /// </summary>
        public byte[] Data64 { get; set; } = new byte[64];

        /// <summary>
        /// A convenience property to hold the current live value.
        /// This value can be derived from Data64 or provided directly by your application.
        /// </summary>
        public object CurrentValue { get; set; }
    }

    /// <summary>
    /// A generic card model that can be used for live room or panel displays.
    /// This model is designed to aggregate or summarize live data.
    /// </summary>
    public class GenericCardModel
    {
        public int Id { get; set; }
        public string CardName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; } // Optional: Direct image/GIF URL
        public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
