using System;

class SecurityAttributes
{
    public SecurityAttributes(IntPtr securityDescriptor)
    {
        this.lpSecurityDescriptor = securityDescriptor;
    }

    //UInt32 nLegnth = 12;
    IntPtr lpSecurityDescriptor;
    //Boolean bInheritHandle = true;
}