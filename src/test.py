import python_epr

if __name__ == "__main__":
    epr = python_epr.read(
        r'G:\Source\CSharp\EasyPathology5\build\Debug\net6.0-windows\saved\乔思源\2022-5-16\1-1-2_肾水样变性_-_40x.epr')
    print(epr)
    python_epr.write(epr, r'G:\1-1-2_肾水样变性_-_40x.epr')
