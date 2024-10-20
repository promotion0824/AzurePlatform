/* eslint-disable @typescript-eslint/no-explicit-any */
// Version 1
// https://stackexchange.github.io/dnscontrol/js
type Ttl = string | number;

// Top Level Functions
declare function REV(address: string): string;
declare function NewRegistrar(name: string, type?: string, meta?: any): string;
declare function NewDnsProvider(name: string, type?: string, meta?: any): string;
declare function DEFAULTS(...modifiers: any[]): void;
declare function D(name: string, registrar: string, ...meta: any[]): void;

// Domain Modifiers
declare function A(name: string, address: string, ...modifiers: any[]): any;
declare function AAAA(name: string, address: string, ...modifiers: any[]): any;
declare function ALIAS(name: string, target: string, ...modifiers: any[]): any;
declare function CAA(name: string, tag: string, value: string, ...modifiers: any[]): any;
declare function CNAME(name: string, target: string, ...modifiers: any[]): any;
declare function DefaultTTL(ttl: Ttl): any;
declare function DnsProvider(name: string, nsCount?: number): any;
declare function MX(name: string, priority: number, target: string, ...modifiers: any[]): any;
declare function NAMESERVER(name: string, ip?: string, ...modifiers: any[]): any;
declare function NO_PURGE(): any;
declare function NS(name: string, target: string, ...modifiers: any[]): any;
declare function PTR(name: string, target: string, ...modifiers: any[]): any;
declare function TLSA(name: string, usage: number, selector: number, type: number, certificate: string, ...modifiers: any[]): any;
declare function TXT(name: string, contents: string, ...modifiers: any[]): any;

declare function SRV(name: string, priority: number, weight: number, port: number, target: string, ...modifiers: any[]): any;

// Record Modifiers
declare function TTL(ttl: Ttl): any;
declare function NAMESERVER_TTL(ttl: Ttl): any;
