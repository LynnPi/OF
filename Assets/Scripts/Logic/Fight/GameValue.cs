
/// <summary>
/// GameValue
/// </summary>
public class GameValue {

    public int Value { get; private set; }

    public int Min { get; private set; }

    public int Max;

    public GameValue( int min, int max ) {
        this.Min = min;
        this.Max = max;
    }


    public bool Enough( int value ) {
        if( value < 0 )
            return false;
        return this.Value >= value;
    }

    public bool IsMax() {
        return this.Value >= Max;
    }

    public bool IsMin() {
        return this.Value >= Min;
    }

    public void SetToMax() {
        this.Value = Max;
    }

    public void SetToMin() {
        this.Value = Min;
    }

    public void SetValue( int value ) {
        this.Value = value;
    }

    public void Change( int value ) {
        int newValue = this.Value + value;
        if( newValue < this.Min )
            newValue = Min;
        else if (newValue == this.Max)
            newValue = Max;

        this.Value = newValue;
    }

    public int GetRest() {
        return this.Max - this.Value;
    }

    public bool ChangeMax(int max) {
        if( this.Min <= this.Max && max > 0 && max != this.Max ) {
            this.Max = max;
            return true;
        }

        return false;
    }
}
