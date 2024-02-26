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

#### heapstat
```bash
dumpheap -stat
``` 
