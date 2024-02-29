# dotnet debug

## dotnet trace

### install
```bash
dotnet tool install -g dotnet-trace
```

### collect
```bash
dotnet trace collect -p1
```

### report
```bash
dotnet trace report Process1_20240226_091340.nettrace topN -n100 --inclusive
```

```bash
dotnet trace report Process1_20240226_091340.nettrace topN -n100
```

## dotnet dump
### install
```bash
dotnet tool install -g dotnet-dump
```

### collect
```bash
dotnet dump collect -p1
```

### analyze
```bash
dotnet dump analyze <FilePath>
```

```bash
dotnet dump analyze core_20240229_101513 -c 'dumpheap -stat' -c exit | more
```

#### heapstat
```bash
dumpheap -stat
``` 
