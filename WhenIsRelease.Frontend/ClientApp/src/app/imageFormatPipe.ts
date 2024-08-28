import { Pipe, PipeTransform } from "@angular/core";

@Pipe({ name: "imageFormat" })
export class ImageFormatPipe implements PipeTransform {
  transform(value: string, args: string): string {

    switch (args) {
      case "mini":
        value = value.replace('/original/', '/square_mini/');
        break;
      case "avatarSq":
        value = value.replace('/original/', '/square_avatar/');
        break;
      case "avatar":
        value = value.replace('/original/', '/scale_avatar/');
        break;
      case "scaleSm":
        value = value.replace('/original/', '/scale_small/');
        break;
      case "scaleMed":
        value = value.replace('/original/', '/scale_medium/');
        break;
      case "scaleLg":
        value = value.replace('/original/', '/scale_large/');
        break;
      case "screenMed":
        value = value.replace('/original/', '/screen_medium/');
        break;
      case "kubrick":
        value = value.replace('/original/', '/screen_kubrick/');
        break;
      default:
        value = value.replace('/original/', args);
        break;
    }

    return value;
  }
}
