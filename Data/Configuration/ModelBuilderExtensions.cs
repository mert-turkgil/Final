using System;
using Final.Entity;
using Microsoft.EntityFrameworkCore;

namespace Final.Data.Configuration
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            // --------------------------------
            // 1) Define static GUIDs to ensure relationships line up
            // --------------------------------
            var companyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var controlRoomToolId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var compressorToolId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            // --------------------------------
            // 2) Seed Company
            // --------------------------------
            modelBuilder.Entity<Final.Entity.Company>().HasData(
                new Final.Entity.Company
                {
                    Id = companyId,
                    Name = "Ciceklisogukhavadeposu",
                    BaseTopic = "ciceklisogukhavadeposu"
                }
            );

            // --------------------------------
            // 3) Seed Tools (Control Room, Compressor, etc.)
            // --------------------------------
            modelBuilder.Entity<Final.Entity.MqttTool>().HasData(
                new Final.Entity.MqttTool
                {
                    Id = controlRoomToolId,
                    CompanyId = companyId,
                    Name = "Control Room",
                    Description = "Main control room for the company"
                },
                new Final.Entity.MqttTool
                {
                    Id = compressorToolId,
                    CompanyId = companyId,
                    Name = "Compressor",
                    Description = "Compressor tool"
                }
            );

            // --------------------------------
            // 4) Seed Static Topics
            // --------------------------------
            var topicRoomStatusId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var topicCompressorId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var topicPwrRqstId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            var topicSetTempId = Guid.Parse("88888888-8888-8888-8888-888888888888");

            modelBuilder.Entity<Final.Entity.MqttTopic>().HasData(
                // Example: ciceklisogukhavadeposu/control_room/room{room}/status
                new Final.Entity.MqttTopic
                {
                    Id = topicRoomStatusId,
                    MqttToolId = controlRoomToolId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = "ciceklisogukhavadeposu/control_room/room{room}/status",
                    DataType = Final.Entity.TopicDataType.Float,
                    HowMany = 1
                },
                // Example: ciceklisogukhavadeposu/control_room/compressor/status
                new Final.Entity.MqttTopic
                {
                    Id = topicCompressorId,
                    MqttToolId = compressorToolId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = "ciceklisogukhavadeposu/control_room/compressor/status",
                    DataType = Final.Entity.TopicDataType.Int64,
                    HowMany = 1
                },
                // Special Case 1: pwr_rqst topic with room number in the template
                new Final.Entity.MqttTopic
                {
                    Id = topicPwrRqstId,
                    MqttToolId = controlRoomToolId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = "pwr_rqst/room{room}/control_room/ciceklisogukhavadeposu",
                    DataType = Final.Entity.TopicDataType.Int64,
                    HowMany = 1
                },
                // Special Case 2: set_temp topic with room number in the template
                new Final.Entity.MqttTopic
                {
                    Id = topicSetTempId,
                    MqttToolId = controlRoomToolId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = "set_temp/room{room}/control_room/ciceklisogukhavadeposu",
                    DataType = Final.Entity.TopicDataType.Float,
                    HowMany = 1
                }
            );

            // --------------------------------
            // 5) Seed 12 Room Temp Topics
            // --------------------------------
            // Here we create one topic for each room (1 to 12) using the temp template.
            var roomTempTopics = new List<Final.Entity.MqttTopic>();
            for (int room = 1; room <= 12; room++)
            {
                // Generate a stable GUID for each room topic.
                // For example, room 1 will get: "00000000-0000-0000-0000-000000000001"
                string hexRoomString = room.ToString("X12"); // 12-digit hexadecimal
                Guid topicId = Guid.Parse($"00000000-0000-0000-0000-{hexRoomString}");

                roomTempTopics.Add(new Final.Entity.MqttTopic
                {
                    Id = topicId,
                    MqttToolId = controlRoomToolId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    // Substitute the room number into the topic template
                    TopicTemplate = $"ciceklisogukhavadeposu/control_room/room{room}/temp",
                    DataType = Final.Entity.TopicDataType.Float,
                    HowMany = 1
                });
            }
            modelBuilder.Entity<Final.Entity.MqttTopic>().HasData(roomTempTopics.ToArray());
        }
    }
}
