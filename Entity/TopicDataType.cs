using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Final.Entity
{
    public enum TopicDataType
    {
        // Signed integer types
        Int8,    // 8-bit signed integer
        Int16,   // 16-bit signed integer
        Int32,   // 32-bit signed integer
        Int64,   // 64-bit signed integer
        
        // Unsigned integer types
        UInt8,   // 8-bit unsigned integer
        UInt16,  // 16-bit unsigned integer
        UInt32,  // 32-bit unsigned integer
        UInt64,  // 64-bit unsigned integer
        
        // Floating-point types
        Float,   // 32-bit floating point
        Double,  // 64-bit floating point
        Decimal, // 128-bit decimal type (precise for financial calculations)
        
        // Boolean
        Boolean, 
        
        // Text/string
        String,  
        
        // 64-byte binary (could be used for large data, e.g. hashing, custom protocols, etc.)
        Byte64  
    }
}