import { Pipe, PipeTransform } from '@angular/core';
@Pipe({name: 'appFileNamePipe'})
export class FileNamePipe implements PipeTransform {
  transform(value: string): string {
    return value.replace(/^.*[\\\/]/, '');
  }
}
