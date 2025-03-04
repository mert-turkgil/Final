using System;
using System.Collections.Generic;
using Final.Entity;
using Final.Identity;
using Microsoft.AspNetCore.Identity;
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
            // We'll seed two tools: "Control Room" and "Compressor"
            // If you intend room topics to be company-level, they will not be associated with any tool.
            var controlRoomToolId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var compressorToolId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            // --------------------------------
            // 2) Seed Company
            // --------------------------------
            modelBuilder.Entity<Company>().HasData(
                new Company
                {
                    Id = companyId,
                    Name = "Ciceklisogukhavadeposu",
                    BaseTopic = "ciceklisogukhavadeposu",
                }
            );

            // --------------------------------
            // 3) Seed Tools
            // --------------------------------
            // Here, "Compressor" is considered a tool.
            // "Control Room" is also seeded but its topics will not be tied to it,
            // making them company-level.
            modelBuilder.Entity<MqttTool>().HasData(
                new MqttTool
                {
                    Id = compressorToolId,
                    CompanyId = companyId,
                    Name = "Compressor",
                    Description = "Compressor tool"
                }
            );

            // --------------------------------
            // 4) Seed Static Topic for the Compressor Tool
            // --------------------------------
            // This topic is associated with the Compressor tool.
            var topicCompressorId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            modelBuilder.Entity<MqttTopic>().HasData(
                new MqttTopic
                {
                    Id = topicCompressorId,
                    MqttToolId = compressorToolId, // tool-level
                    CompanyId = companyId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = "ciceklisogukhavadeposu/control_room/compressor/status",
                    DataType = TopicDataType.Int64,
                    TopicPurpose = TopicPurpose.Subscription,
                    HowMany = 1
                }
            );

            // --------------------------------
            // 5a) Seed Room Status Data (pwr_rqst topics)
            // --------------------------------
            // These topics are now company-level (MqttToolId = null)
            var roomStatusData = new List<MqttTopic>();
            for (int room = 1; room <= 12; room++)
            {
                string hexRoomString = room.ToString("X12"); // 12-digit hexadecimal string
                Guid topicId = Guid.Parse($"AAAAAAA0-AAAA-AAAA-AAAA-{hexRoomString}");
                roomStatusData.Add(new MqttTopic
                {
                    Id = topicId,
                    MqttToolId = null, // Company-level topic
                    CompanyId = companyId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = $"pwr_rqst/room{room}/control_room/ciceklisogukhavadeposu",
                    DataType = TopicDataType.Int64,
                    TopicPurpose = TopicPurpose.Sending,
                    HowMany = 12
                });
            }
            modelBuilder.Entity<MqttTopic>().HasData(roomStatusData.ToArray());

            // --------------------------------
            // 5b) Seed 12 set_temp/room Topics
            // --------------------------------
            var roomSendData = new List<MqttTopic>();
            for (int room = 1; room <= 12; room++)
            {
                string hexRoomString = room.ToString("X12");
                Guid topicId = Guid.Parse($"BBBBBBBB-BBBB-BBBB-BBBB-{hexRoomString}");
                roomSendData.Add(new MqttTopic
                {
                    Id = topicId,
                    MqttToolId = null, // Company-level
                    CompanyId = companyId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = $"set_temp/room{room}/control_room/ciceklisogukhavadeposu",
                    DataType = TopicDataType.Float,
                    TopicPurpose = TopicPurpose.Sending,
                    HowMany = 12
                });
            }
            modelBuilder.Entity<MqttTopic>().HasData(roomSendData.ToArray());

            // --------------------------------
            // 5c) Seed 12 Room Status Topics
            // --------------------------------
            var roomStatusTopics = new List<MqttTopic>();
            for (int room = 1; room <= 12; room++)
            {
                string hexRoomString = room.ToString("X12");
                Guid topicId = Guid.Parse($"CCCCCCCC-CCCC-CCCC-CCCC-{hexRoomString}");
                roomStatusTopics.Add(new MqttTopic
                {
                    Id = topicId,
                    MqttToolId = null, // Company-level
                    CompanyId = companyId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = $"ciceklisogukhavadeposu/control_room/room{room}/status",
                    DataType = TopicDataType.Float,
                    TopicPurpose = TopicPurpose.Subscription,
                    HowMany = 12
                });
            }
            modelBuilder.Entity<MqttTopic>().HasData(roomStatusTopics.ToArray());

            // --------------------------------
            // 5d) Seed 12 Room Temp Topics
            // --------------------------------
            var roomTempTopics = new List<MqttTopic>();
            for (int room = 1; room <= 12; room++)
            {
                string hexRoomString = room.ToString("X12");
                Guid topicId = Guid.Parse($"00000000-0000-0000-0000-{hexRoomString}");
                roomTempTopics.Add(new MqttTopic
                {
                    Id = topicId,
                    MqttToolId = null, // Company-level
                    CompanyId = companyId,
                    BaseTopic = "ciceklisogukhavadeposu",
                    TopicTemplate = $"ciceklisogukhavadeposu/control_room/room{room}/temp",
                    DataType = TopicDataType.Float,
                    TopicPurpose = TopicPurpose.Subscription,
                    HowMany = 1
                });
            }
            modelBuilder.Entity<MqttTopic>().HasData(roomTempTopics.ToArray());

            // --------------------------------
            // 6) Seed Role for the Company and Bind It
            // --------------------------------
            var roleId = "role-ciceklisogukhavadeposu";
            // The actual role record should be created in ApplicationDbContext or via RoleManager.
            // Here, we only seed the CompanyRole record that references that role.
            modelBuilder.Entity<CompanyRole>().HasData(
                new CompanyRole
                {
                    Id = 1,
                    CompanyId = companyId,
                    RoleId = roleId
                }
            );
        }
    }
}
