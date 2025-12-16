
import { OnInit, ChangeDetectorRef } from '@angular/core';
import { Component } from '@angular/core';
import { GaragesService } from '../../services/garages';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-garages',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSelectModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './garages.html',
  styleUrl: './garages.css',
})
export class Garages implements OnInit {
    constructor(
      private garagesService: GaragesService,
      private snackBar: MatSnackBar,
      private cdr: ChangeDetectorRef
    ) {}

    ngOnInit() {
      this.loadGaragesFromGov();
      this.loadGaragesFromDb();
    }
    allSelectedExist(): boolean {
    if (!this.selectedGarages.length) return false;
    const existingIds = new Set(this.garagesFromDb.map(g => g.externalId || g.id));
    return this.selectedGarages.every(g => existingIds.has(g.externalId || g.id));
  }
    addSelectedGarages() {
      if (this.selectedGarages.length === 0) return;

      const existingIds = new Set(this.garagesFromDb.map(g => g.externalId || g.id));
      const newGarages = this.selectedGarages.filter(
        g => !existingIds.has(g.externalId || g.id)
      );

      if (newGarages.length === 0) {
        this.snackBar.open('All selected garages already exist in the database', 'Close', { duration: 3000 });
        return;
      }

      if (newGarages.length < this.selectedGarages.length) {
        this.snackBar.open('Some garages already exist and were not added', 'Close', { duration: 3000 });
      }

      this.loading = true;
      this.garagesService.addGaragesToDb(newGarages).subscribe({
        next: (res) => {
          setTimeout(() => {
            this.snackBar.open('Garages added successfully', 'Close', { duration: 3000 });
            this.selectedGarages = [];
            this.loadGaragesFromDb();
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
        error: () => {
          this.snackBar.open('Failed to add garages', 'Close', { duration: 3000 });
          setTimeout(() => this.loading = false);
        }
      });
    }
  garagesFromGov: any[] = [];
  garagesFromDb: any[] = [];
  selectedGarages: any[] = [];
  loading = false;

  loadGaragesFromGov() {
    this.loading = true;
    this.garagesService.getGaragesFromGov().subscribe({
        next: (data) => {
          setTimeout(() => {
            this.garagesFromGov = data;
            this.loading = false;
            this.cdr.detectChanges();
          });
      },
      error: () => {
        this.snackBar.open('Failed to load garages from gov', 'Close', { duration: 3000 });
        setTimeout(() => this.loading = false);
      }
    });
  }

  loadGaragesFromDb() {
    this.loading = true;
    this.garagesService.getGaragesFromDb().subscribe({
        next: (data) => {
          setTimeout(() => {
            this.garagesFromDb = data;
            this.loading = false;
            this.cdr.detectChanges();
          });
      },
      error: () => {
        this.snackBar.open('Failed to load garages from DB', 'Close', { duration: 3000 });
        setTimeout(() => this.loading = false);
      }
    });
  }
}
